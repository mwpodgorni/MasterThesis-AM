import pandas as pd
import matplotlib.pyplot as plt

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

# Use only columns 9 to 21 (zero-based index 8 to 20) to infer correct answers
combined_df = pd.concat([game_df, paper_df], ignore_index=True)
question_columns = combined_df.columns[8:21]

# Get most common answer (mode) for each question, excluding 'Not applicable'
correct_answers = {}
for col in question_columns:
    mode_series = combined_df[col][combined_df[col] != 'Not applicable'].mode()
    if not mode_series.empty:
        correct_answers[col] = mode_series.iloc[0]

# Raw score: total correct answers per participant
def compute_raw_average_score(df):
    scores = []
    for _, row in df.iterrows():
        score = 0
        for question, correct_answer in correct_answers.items():
            if pd.notna(row.get(question)) and row[question] != 'Not applicable':
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
            if pd.notna(row.get(question)) and row[question] != 'Not applicable':
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

# Visualization
groups = ['Game', 'Paper']
perceived_understanding = [game_averages.iloc[0], paper_averages.iloc[0]]
format_helpfulness = [game_averages.iloc[1], paper_averages.iloc[1]]
raw_scores = [game_raw, paper_raw]
normalized_scores = [game_norm, paper_norm]

fig, axs = plt.subplots(2, 2, figsize=(10, 8))
fig.suptitle('Main Testing Results: Game vs Paper Groups', fontsize=14)

axs[0, 0].bar(groups, perceived_understanding)
axs[0, 0].set_title('Perceived Understanding')
axs[0, 0].set_ylim(0, 5)
axs[0, 0].set_ylabel('Avg. Rating (1–5)')

axs[0, 1].bar(groups, format_helpfulness)
axs[0, 1].set_title('Format Helpfulness')
axs[0, 1].set_ylim(0, 5)
axs[0, 1].set_ylabel('Avg. Rating (1–5)')

axs[1, 0].bar(groups, raw_scores)
axs[1, 0].set_title('Raw Avg. Knowledge Score')
axs[1, 0].set_ylim(0, 13)
axs[1, 0].set_ylabel('Correct Answers (max 13)')

axs[1, 1].bar(groups, normalized_scores)
axs[1, 1].set_title('Normalized Knowledge Accuracy')
axs[1, 1].set_ylim(0, 1)
axs[1, 1].set_ylabel('Proportion Correct')

plt.tight_layout(rect=[0, 0, 1, 0.95])
plt.show()
