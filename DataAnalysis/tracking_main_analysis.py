import pandas as pd

file_path = 'TrackingMain.csv'
df = pd.read_csv(file_path)

df['Action Name'] = df['Action Name'].fillna('')

help_actions = df[df['Action Name'].str.startswith('Help')]
session_help_counts = help_actions.groupby('Session ID')['Count'].sum()
average_help_actions = session_help_counts.mean()
print(f"Average number of 'Help' actions per session: {average_help_actions:.2f}")

df['Timer Name'] = df['Timer Name'].fillna('')
df['Duration'] = pd.to_numeric(df['Duration'], errors='coerce')

stage_one = df[df['Timer Name'] == 'StageOneTime']['Duration']
stage_two = df[df['Timer Name'] == 'StageTwoTime']['Duration']
paper_open = df[df['Timer Name'] == 'PaperOpen']['Duration']

print(f"StageOneTime - Total: {stage_one.sum():.2f}, Average: {stage_one.mean():.2f}")
print(f"StageTwoTime - Total: {stage_two.sum():.2f}, Average: {stage_two.mean():.2f}")
print(f"Average 'PaperOpen' time: {paper_open.mean():.2f}")
