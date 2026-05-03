# EscapeGame Netlify デプロイ

## 概要
WebGLビルドをGitHubにプッシュしてNetlifyへデプロイする。

## 前提条件
- Unity Edit mode であること（Play mode 中は不可）
- `npx netlify` が使えること（`npx netlify status` で確認）

## 手順

### 1. WebGLビルド
Unity エディターで実行：
```
EscapeGame/Build/Build WebGL
```
ビルド完了後、`Builds/WebGL/Build/WebGL.wasm.br` が 0 バイトでないことを確認。

### 2. GitHubにプッシュ
```bash
cd /Users/ohori/Documents/Claude/EscapeGame
git add Builds/WebGL/
git commit -m "Update WebGL build"
git push
```
※ git push には GitHub トークンが必要。トークンが切れた場合は `gh auth login` で再認証。

### 3. Netlifyへデプロイ
```bash
npx netlify deploy --prod --dir=Builds/WebGL
```

## サイト情報
- **公開URL**: https://escapegame01.netlify.app
- **GitHubリポジトリ**: https://github.com/takahiroohori910/escape-game
- **Netlify サイトID**: 6666eb24-9a36-4ed2-930a-76978e92dab4

## トラブルシューティング

### Netlifyにログインできない
```bash
npx netlify login
```
ブラウザが開くのでGitHubアカウントで認証する。

### git pushが弾かれる
```bash
gh auth login
gh auth token  # トークンを取得
# トークンをリモートURLに埋め込んでプッシュ
git remote set-url origin "https://<token>@github.com/takahiroohori910/escape-game.git"
git push
git remote set-url origin "https://github.com/takahiroohori910/escape-game.git"
```

### WebGL.wasm.br が 0 バイトのまま
ビルドがまだ圧縮中。完了まで待つ：
```bash
until [ "$(stat -f%z Builds/WebGL/Build/WebGL.wasm.br)" -gt 0 ]; do sleep 5; done && echo "完了"
```
