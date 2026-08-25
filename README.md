# AKSCS14App

Arimitsu ga Kangaeta Saikyo no C#14App

## 構成

## 開発環境セットアップ

コミット時に `dotnet format` による自動フォーマットを有効にするため、初回のみ以下を実行してください（Dev Container 利用時は `postCreateCommand` で自動実行されます）。

```bash
dotnet tool restore
dotnet husky install
```

以降、`git commit` のたびにステージ済みの `*.cs` ファイルが `.editorconfig` の規約に沿って自動整形され、コミットに含まれます。
