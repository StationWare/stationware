#!/usr/bin/env python3

import requests
import yaml
import re
import os

# handy regex queries
headerRegex = re.compile('^\s*(?::cl:|🆑) *([a-z0-9_\- ]+)?\s+', re.I | re.M)
entryRegex = re.compile('^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?', re.RegexFlag.I | re.RegexFlag.M)
commentRegex = re.compile('<!--.*?--', re.S)

# environ
repo = os.environ.get('GITHUB_REPOSITORY') or 'StationWare/StationWare'
pr_number = os.environ.get('PR_NUMBER') or 177
params = {'Authorization': f'Bearer {os.environ.get("GITHUB_TOKEN")}'}
cl_file_path = 'Resources/Changelog/Changelog.yml'

# functions
def get_changes(pr_body):
    entries = []

    for entry in re.findall(entryRegex, pr_body):
        entries.append({"message": entry[1], "type": entry[0]})

    return entries

def get_newest_log():
    with open(cl_file_path, 'r') as file:
        yml = yaml.load(file, yaml.Loader)
        return yml['Entries'][-1]

# fetch
pr = requests.get(f'https://api.github.com/repos/{repo}/pulls/{pr_number}').json()
merged_at = pr['merged_at']

if merged_at == None:
    print("not merged")
    exit()

body = re.sub(commentRegex, '', pr['body'])
user = pr['user']

author_name = re.findall(headerRegex, body)[0] or user['login']

entries = get_changes(body)
yml_entry = {
    'author': author_name,
    'changes': entries,
    'id': get_newest_log()['id'] + 1,
    'time': merged_at
}

with open(cl_file_path, 'r+') as file:
    # vars
    lines = file.read()
    data = yaml.load(lines, yaml.Loader)

    # entry
    entries = data["Entries"]
    entries.pop(0)
    entries.append(yml_entry)

    # wipe
    file.seek(0)
    file.truncate(0)

    # write
    file.write(yaml.dump(data))
