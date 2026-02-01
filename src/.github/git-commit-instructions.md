# Git Commit Standards
All commits must be local. Do not include "Signed-off-by".

## Format
Use the following format:
`[Migration] <Action>: <Brief Description> (Task: <Plan Item Name>)`

## Rules
- Use the imperative mood ("Add", not "Added").
- Keep the subject line under 72 characters.
- Do not use emojis.
- Example: `[Migration] Fix: Update NServiceBus transport to Azure Service Bus (Task: Transport Setup)`

## Context
If a change was reverted or fixed, mention why:
Example: `[Migration] Fix: Corrected connection string format after build failure`