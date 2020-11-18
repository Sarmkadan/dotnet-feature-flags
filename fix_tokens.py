import os
import re

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find public async Task methods
    # public async Task MethodName(args...)
    # public async Task<Type> MethodName(args...)
    
    def repl(m):
        # m.group(0) is the full match, like: public async Task<IActionResult> EvaluateFeatureFlag([FromBody] EvaluationRequest request)
        full_match = m.group(0)
        
        # If it already has CancellationToken, skip
        if 'CancellationToken' in full_match:
            return full_match
            
        # Insert "CancellationToken cancellationToken = default" before the last ')'
        # Need to handle empty args () or existing args (...)
        
        # Find the last ')'
        last_paren_idx = full_match.rfind(')')
        if last_paren_idx == -1:
            return full_match
            
        # Is it empty args?
        open_paren_idx = full_match.rfind('(', 0, last_paren_idx)
        
        inside_args = full_match[open_paren_idx+1:last_paren_idx].strip()
        
        if not inside_args:
            insertion = "CancellationToken cancellationToken = default"
        else:
            insertion = ", CancellationToken cancellationToken = default"
            
        return full_match[:last_paren_idx] + insertion + full_match[last_paren_idx:]

    # Match multiline signatures:
    # public async Task(\s*<[^>]+>)?\s+\w+\s*\([^)]*\)
    pattern = re.compile(r'public\s+async\s+Task(?:<[^>]+>)?\s+\w+\s*\([^)]*\)', re.MULTILINE)
    
    new_content = pattern.sub(repl, content)
    
    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

for root, _, files in os.walk('src/FeatureFlags'):
    for f in files:
        if f.endswith('.cs'):
            process_file(os.path.join(root, f))
