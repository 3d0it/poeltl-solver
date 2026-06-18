import argparse
import time
from pathlib import Path


TEAM_META = {
    "ATL": ("East", "Southeast"),
    "BOS": ("East", "Atlantic"),
    "BKN": ("East", "Atlantic"),
    "CHA": ("East", "Southeast"),
    "CHI": ("East", "Central"),
    "CLE": ("East", "Central"),
    "DAL": ("West", "Southwest"),
    "DEN": ("West", "Northwest"),
    "DET": ("East", "Central"),
    "GSW": ("West", "Pacific"),
    "HOU": ("West", "Southwest"),
    "IND": ("East", "Central"),
    "LAC": ("West", "Pacific"),
    "LAL": ("West", "Pacific"),
    "MEM": ("West", "Southwest"),
    "MIA": ("East", "Southeast"),
    "MIL": ("East", "Central"),
    "MIN": ("West", "Northwest"),
    "NOP": ("West", "Southwest"),
    "NYK": ("East", "Atlantic"),
    "OKC": ("West", "Northwest"),
    "ORL": ("East", "Southeast"),
    "PHI": ("East", "Atlantic"),
    "PHX": ("West", "Pacific"),
    "POR": ("West", "Northwest"),
    "SAC": ("West", "Pacific"),
    "SAS": ("West", "Southwest"),
    "TOR": ("East", "Atlantic"),
    "UTA": ("West", "Northwest"),
    "WAS": ("East", "Southeast"),
}


def height_to_inches(height: str) -> int | None:
    """
    Converts a height string like '6-11' to total inches.
    6-11 = 6*12 + 11 = 83.
    """
    if not isinstance(height, str) or "-" not in height:
        return None

    feet, inches = height.split("-")
    return int(feet) * 12 + int(inches)


def normalize_height(height: str) -> str:
    """
    Keeps height in the NBA/Poeltl format: 6-11, 6-3, etc.
    """
    if not isinstance(height, str):
        return ""

    return height.strip()


def normalize_position(position: str) -> str:
    """
    Keeps the position as returned by NBA.com.
    Possible examples: Guard, Forward, Center, Guard-Forward, Forward-Center.
    """
    if not isinstance(position, str):
        return ""

    return position.strip()


def safe_age(value) -> int | None:
    import pandas as pd

    try:
        if pd.isna(value):
            return None
        return int(value)
    except (TypeError, ValueError):
        return None


def normalize_number(value) -> str | None:
    """
    Some players can have an empty or missing jersey number.
    Jersey 00 is valid and must stay distinct from 0.
    """
    import math

    if value is None:
        return None

    if isinstance(value, float) and math.isnan(value):
        return None

    if isinstance(value, str):
        number = value.strip()
        if not number:
            return None
        if number.endswith(".0") and number[:-2].isdigit():
            return str(int(number[:-2]))
        return number

    if isinstance(value, float) and value.is_integer():
        return str(int(value))

    number = str(value).strip()
    return number or None


def parse_args():
    parser = argparse.ArgumentParser(description="Download NBA roster data for Poeltl Solver.")
    parser.add_argument(
        "--season",
        default="2025-26",
        help="NBA season to download, for example 2025-26.",
    )
    return parser.parse_args()


def main():
    args = parse_args()

    import pandas as pd
    from nba_api.stats.endpoints import commonteamroster
    from nba_api.stats.static import teams

    all_rows = []

    nba_teams = teams.get_teams()

    for team in nba_teams:
        team_id = team["id"]
        abbreviation = team["abbreviation"]

        if abbreviation not in TEAM_META:
            print(f"Unsupported team: {abbreviation}")
            continue

        conference, division = TEAM_META[abbreviation]

        print(f"Downloading roster {abbreviation}...")

        roster = commonteamroster.CommonTeamRoster(
            team_id=team_id,
            season=args.season,
            league_id_nullable="00",
        ).get_data_frames()[0]

        for _, row in roster.iterrows():
            height = normalize_height(row["HEIGHT"])

            all_rows.append(
                {
                    "Name": row["PLAYER"],
                    "Team": abbreviation,
                    "Conference": conference,
                    "Division": division,
                    "Position": normalize_position(row["POSITION"]),
                    "Height": height,
                    "HeightInches": height_to_inches(height),
                    "Age": safe_age(row["AGE"]),
                    "Number": normalize_number(row["NUM"]),
                }
            )

        # Avoid hammering NBA.com with 30 consecutive requests.
        time.sleep(1)

    df = pd.DataFrame(all_rows)

    df = df[
        [
            "Name",
            "Team",
            "Conference",
            "Division",
            "Position",
            "Height",
            "HeightInches",
            "Age",
            "Number",
        ]
    ]

    df = df.sort_values(["Team", "Name"]).reset_index(drop=True)

    script_dir = Path(__file__).resolve().parent
    output_path = script_dir / "poeltl_players.csv"

    df.to_csv(output_path, index=False)

    print()
    print(f"Created {output_path} with {len(df)} players")
    print()
    print(df.head(10))


if __name__ == "__main__":
    main()
