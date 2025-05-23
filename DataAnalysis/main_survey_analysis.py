import pandas as pd

# Load the CSV files
game_df = pd.read_csv('MainSurveyGame.csv')
paper_df = pd.read_csv('MainSurveyPaper.csv')

# Columns to analyze (perceived understanding and format effectiveness)
rating_columns = [
    'I feel that I understand the core concepts presented in the game/paper. ',
    'I believe the format I experienced helped me learn presented concepts.'
]

# Convert to numeric and compute averages
game_averages = game_df[rating_columns].apply(pd.to_numeric, errors='coerce').mean()
paper_averages = paper_df[rating_columns].apply(pd.to_numeric, errors='coerce').mean()

print("Game group averages:")
print(game_averages)
print("\nPaper group averages:")
print(paper_averages)

# Use only columns 9 to 21 (zero-based index 8 to 20)
combined_df = pd.concat([game_df, paper_df], ignore_index=True)
question_columns = combined_df.columns[8:21]

# Get most common (mode) answer for each question, excluding 'Not applicable'
correct_answers = {}
for col in question_columns:
    mode_series = combined_df[col][combined_df[col] != 'Not applicable'].mode()
    if not mode_series.empty:
        correct_answers[col] = mode_series.iloc[0]

# Raw score: total correct answers per participant (max 13)
def compute_raw_average_score(df):
    scores = []
    for _, row in df.iterrows():
        score = 0
        for question, correct_answer in correct_answers.items():
            if question in row and pd.notna(row[question]) and row[question] != 'Not applicable':
                if row[question] == correct_answer:
                    score += 1
        scores.append(score)
    return sum(scores) / len(scores) if scores else 0

# Normalized score: correct/attempted per participant
def compute_normalized_average_score(df):
    normalized_scores = []
    for _, row in df.iterrows():
        score = 0
        answered = 0
        for question, correct_answer in correct_answers.items():
            if question in row and pd.notna(row[question]) and row[question] != 'Not applicable':
                answered += 1
                if row[question] == correct_answer:
                    score += 1
        if answered > 0:
            normalized_scores.append(score / answered)
    return sum(normalized_scores) / len(normalized_scores) if normalized_scores else 0

# Compute both scores
game_raw = compute_raw_average_score(game_df)
paper_raw = compute_raw_average_score(paper_df)
game_norm = compute_normalized_average_score(game_df)
paper_norm = compute_normalized_average_score(paper_df)

print(f"\nRaw average correct answers per participant (out of 13):")
print(f"Game group: {game_raw:.2f}/13")
print(f"Paper group: {paper_raw:.2f}/13")

print(f"\nNormalized average correct answer rate per participant:")
print(f"Game group: {game_norm:.2f}")
print(f"Paper group: {paper_norm:.2f}")
