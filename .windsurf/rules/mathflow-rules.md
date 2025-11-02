---
trigger: always_on
description: 
globs: 
---

<General>
- This application is meant to be as simple as possible. ``AVOID``over-engineering everytime that it is possible.
</General>

<Documentation>
- Every time that you modify files, check if the documentation files, i.e., *.md, needs to be updated: if yes, do it.
</Documentation>

<Tests>
- For mocking purpose, use NSubstitute. ``AVOID``using Moq.
</Tests>