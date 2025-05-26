import pandas as pd
import matplotlib.pyplot as plt

file_path = 'TrackingPreliminary.csv'
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

avg_stage_one = stage_one.mean()
avg_stage_two = stage_two.mean()

print(f"StageOneTime - Total: {stage_one.sum():.2f}, Average: {avg_stage_one:.2f}")
print(f"StageTwoTime - Total: {stage_two.sum():.2f}, Average: {avg_stage_two:.2f}")

# RGB color converted to matplotlib format
bar_color = (66/255, 133/255, 244/255)

# Plot charts
plt.figure(figsize=(10, 4))

# Chart 1: Average completion times
plt.subplot(1, 2, 1)
plt.bar(['Neural Network', 'Reinforcement Learning'], [avg_stage_one, avg_stage_two], color=bar_color)
plt.ylabel('Seconds')
plt.title('Average Completion Time')

# Chart 2: Help usage
plt.subplot(1, 2, 2)
plt.bar(['Help Panel Opens'], [average_help_actions], color=bar_color)
plt.ylabel('Count')
plt.title('Average Help Panel Usage')

plt.tight_layout()
plt.show()