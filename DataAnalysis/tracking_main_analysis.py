import pandas as pd
import matplotlib.pyplot as plt

file_path = 'TrackingMain.csv'
df = pd.read_csv(file_path)

df['Action Name'] = df['Action Name'].fillna('')
df['Timer Name'] = df['Timer Name'].fillna('')
df['Duration'] = pd.to_numeric(df['Duration'], errors='coerce')

# Help panel usage
help_actions = df[df['Action Name'].str.startswith('Help')]
session_help_counts = help_actions.groupby('Session ID')['Count'].sum()
average_help_actions = session_help_counts.mean()
print(f"Average number of 'Help' actions per session: {average_help_actions:.2f}")

# Completion times
stage_one = df[df['Timer Name'] == 'StageOneTime']['Duration']
stage_two = df[df['Timer Name'] == 'StageTwoTime']['Duration']
paper_time = df[df['Timer Name'] == 'PaperOpen']['Duration']

avg_stage_one = stage_one.mean()
avg_stage_two = stage_two.mean()
avg_paper_time = paper_time.mean()

print(f"StageOneTime - Total: {stage_one.sum():.2f}, Average: {avg_stage_one:.2f}")
print(f"StageTwoTime - Total: {stage_two.sum():.2f}, Average: {avg_stage_two:.2f}")
print(f"PaperReadingTime - Total: {paper_time.sum():.2f}, Average: {avg_paper_time:.2f}")

# RGB color
bar_color = (66/255, 133/255, 244/255)

# Plot charts
plt.figure(figsize=(10, 4))

# Chart 1: Average completion times
plt.subplot(1, 2, 1)
plt.bar(['Neural Network', 'Reinforcement Learning', 'Paper'], [avg_stage_one, avg_stage_two, avg_paper_time], color=bar_color)
plt.ylabel('Seconds')
plt.title('Average Completion Time')

# Chart 2: Help usage
plt.subplot(1, 2, 2)
plt.bar(['Help Panel Opens'], [average_help_actions], color=bar_color)
plt.ylabel('Count')
plt.title('Average Help Panel Usage')

plt.tight_layout()
plt.show()