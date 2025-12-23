---
description: Run Axiom Engine unit tests in Unity batch mode
---

To run the project tests and verify system integrity:

1. Ensure the Unity Editor is closed (only one instance can access the project at a time).
// turbo
2. Run the test script:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/RunTests.ps1 -TestMode EditMode
```

3. View the results summary in the console or check `TestResults.xml` for detailed logs.
