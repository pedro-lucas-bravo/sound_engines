import psutil
import time

def find_procs_by_name(name):
    "Return a list of processes matching 'name'."
    ls = []
    for proc in psutil.process_iter(['pid', 'name']):
        if proc.info['name'] == name:
            ls.append(proc)
    return ls

process_name = "csound.exe" # Adjust depending on your process name

while True:
    instances = find_procs_by_name(process_name)
    total_cpu_usage = 0
    total_mem_usage = 0
    for instance in instances:
        total_cpu_usage += instance.cpu_percent(interval=1)
        total_mem_usage += instance.memory_info().rss
        
    print(f"Total CPU Usage: {total_cpu_usage}%, Total Memory Usage: {total_mem_usage / (1024 * 1024)}MB")
    time.sleep(1)  # Adjust how often you want to check