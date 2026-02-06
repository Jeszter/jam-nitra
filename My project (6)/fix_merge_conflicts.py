#!/usr/bin/env python3
"""
Script to fix merge conflicts in Unity scene files by keeping both versions.
For Unity YAML files, we need to keep both HEAD and incoming changes since they
typically represent different objects that should both exist.
"""

import re

def fix_merge_conflicts(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Pattern to match merge conflict blocks
    # <<<<<<< HEAD
    # ... HEAD content ...
    # =======
    # ... incoming content ...
    # >>>>>>> commit_hash
    
    conflict_pattern = re.compile(
        r'<<<<<<< HEAD\n(.*?)\n=======\n(.*?)\n>>>>>>> [^\n]+',
        re.DOTALL
    )
    
    def resolve_conflict(match):
        head_content = match.group(1)
        incoming_content = match.group(2)
        
        # For Unity scene files, we typically want to keep both versions
        # since they represent different objects
        # However, we need to be smart about it - if they're the same type of definition,
        # we keep both. If one is empty, we keep the non-empty one.
        
        head_stripped = head_content.strip()
        incoming_stripped = incoming_content.strip()
        
        if not head_stripped:
            return incoming_stripped
        if not incoming_stripped:
            return head_stripped
        
        # Both have content - keep both
        return head_stripped + '\n' + incoming_stripped
    
    # Apply the fix
    fixed_content = conflict_pattern.sub(resolve_conflict, content)
    
    # Write back
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(fixed_content)
    
    print(f"Fixed merge conflicts in {file_path}")

if __name__ == '__main__':
    fix_merge_conflicts('Assets/Scenes/GameScene.unity')
