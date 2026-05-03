# EscapeGame 引き継ぎ指示書

作成日: 2026-04-29  
Unity 6 (6000.4.3f1) / URP / macOS / WebGL

---

## 最初にやること（必読）

1. **CLAUDE.md を読む**（プロジェクトルートにある）
2. **`.claude/unity-patterns.md` を読む**（NGパターン集）
3. **Play Mode が開いていたら必ず先に閉じる**（`EscapeGame/Play Mode/Exit Play Mode`）

---

## プロジェクト概要

2部屋構成の脱出ゲーム（WebGL）。シーン名: `StudyRoom.unity`

| 項目 | 内容 |
|---|---|
| Room1 | 書斎（x=0付近）。本棚・机・暖炉の3パズルで鍵入手 → Room2へ |
| Room2 | 祭壇の間（world z=6〜18）。ステンドグラス・食器棚・燭台・肖像画・祭壇の5パズル |
| 正解コード | DisplayCabinet="2314"、Altar="7429" |
| 主要Editorメニュー | `EscapeGame/Setup/` 以下に各セットアップスクリプト |

---

## 現在の実装状態

### 完了済み
- Room1パズル3本（本棚・机・暖炉）
- Room2パズル5本（ステンドグラス・食器棚・燭台・肖像画・祭壇）
- 鍵システム（Room1クリア → 鍵入手 → 扉通過 → Room2）
- HintUI（エリア別ヒント）
- フォント：4096×4096 Dynamic TMP フォントアセット適用済み（258オブジェクト）

### 未検証（Play Modeで必ず確認すること）
- テキスト文字化けが解消されたか（フォントアトラスを1024→4096に拡張した）
- MENUボタンが正しく全体画面に戻るか（下記「直近のバグ修正」参照）
- Room2の各パズルが動作するか

---

## 直近のバグ修正（このセッションで対応）

### 1. MENUボタン → ヒント表示バグ

**症状**：ステンドグラス画面でMENUを押すとヒントが表示されて全体画面に戻れない  
**原因**：MenuButtonのonClickが永続配線されていなかった（BackButton.Start()のAddListenerは状況によって機能しないことがある）

**修正内容**：
- `HintButtonWiring.cs` → `Wire Buttons` メニューに統合
- MenuButton → `RoomViewController.MoveToCurrentOverview()` に**永続配線**済み
- HintButton → `HintUI.Toggle()` に**永続配線**済み（重複防止処理あり）
- `HintUI.Update()` → エリアが変わったらヒントを自動クローズするよう修正

**再配線が必要な場合**：`EscapeGame/Setup/Wire Buttons` を実行する

### 2. フォント文字化け

**症状**：漢字が表示されない（スペースになる）  
**原因**：TMP Dynamic フォントアトラスが 1024×1024 で溢れていた  
**修正**：`JapaneseFontSetup.cs` のアトラスを **4096×4096** に変更、再作成済み

**文字化け再発時の手順**：
1. `EscapeGame/Setup/Create Japanese Font Asset`
2. `EscapeGame/Setup/Apply Japanese Font to All TMP`

---

## 重要なファイル一覧

```
Assets/_Project/
├── Editor/
│   ├── HintButtonWiring.cs       ← Wire Buttons（MENU・？ボタン永続配線）
│   ├── JapaneseFontSetup.cs      ← フォント作成・適用
│   ├── Phase1Setup.cs            ← Room1基本セットアップ
│   ├── Phase2BackSetup.cs        ← タイトル・タイマー・AudioManager等
│   ├── Phase2FrontSetup.cs       ← HintUI・PopupUI・NoteUI等
│   ├── Room2SetupEditor.cs       ← Room2の建築・照明・パズルUI配線
│   └── InteractableSetup.cs      ← インタラクタブルオブジェクト配線
├── Scripts/
│   ├── Core/                     ← SingletonMonoBehaviour・Manager群
│   └── Game/
│       ├── RoomViewController.cs ← カメラ移動・エリア管理
│       ├── HintUI.cs             ← 【Singleton化済み】エリア別ヒント
│       ├── BackButton.cs         ← MENUボタン（Start()でAddListenerも保持）
│       └── Room1DoorInteractable.cs ← 扉インタラクション
└── Scenes/
    └── StudyRoom.unity           ← メインシーン
```

---

## 既知の問題（未対応）

### Room2クオリティ
- ステンドグラスのコライダー（クリック判定）が正しく機能しているか未確認
- 暗すぎる箇所がある可能性（Room2のライティングは前セッションで改善済みだが未検証）

### UI/UX
- 「？」ボタンを何度押せば閉じるかプレイヤーが分からない可能性（3回押す必要がある）
- NoteUI（説明板など）を閉じた後、どこをクリックすれば次に進めるかわかりにくい可能性

---

## セットアップスクリプト実行順序（シーン再構築が必要な場合）

1. `EscapeGame/Setup/Phase1 Setup`
2. `EscapeGame/Setup/Phase2 Back Setup`
3. `EscapeGame/Setup/Phase2 Front Setup`
4. `EscapeGame/Setup/Wire Buttons`
5. `EscapeGame/Setup/Create Japanese Font Asset`
6. `EscapeGame/Setup/Apply Japanese Font to All TMP`
7. `EscapeGame/Setup/Setup Room2`（Room2のみ）

---

## NGパターン（必ず守ること）

詳細は `.claude/unity-patterns.md` を参照。主要なものだけ抜粋：

| NG | OK |
|---|---|
| `AddListener` でEditorスクリプトから配線 | `UnityEventTools.AddPersistentListener` |
| `FindObjectsByType<T>(FindObjectsSortMode.None)` | `FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)` |
| `[MenuItem]` にデフォルト引数つきメソッドを使う | 引数なしラッパーメソッドを別途作る |
| Play Mode中に作業する | 必ず `Exit Play Mode` してから編集 |
| Wire Buttons を複数回実行 | 実行前に既存リスナーをClearArrayで削除（実装済み） |
