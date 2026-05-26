# EscapeGame プロジェクト ルール

## Unity 作業ルール

- **プレイモード中は作業しない** — スクリプト変更・エディタースクリプト実行は必ず Edit mode で行う。Play mode 中のリコンパイルは反映が不安定になる。
- **作業前に必ず Exit Play Mode を実行すること** — `EscapeGame/Play Mode/Exit Play Mode` メニューで停止してから編集・リコンパイルを行う。
- **テスト時の手順** — Edit mode で編集 → リコンパイル → `EscapeGame/Play Mode/Enter Play Mode` でテスト → 確認後 Exit。
- **新規 .cs ファイル作成後** — 必ず `Assets/Refresh` してから Recompile すること。

## デバッグ原則

1. **症状が出たらまず `unity-patterns.md` を確認する** — 既知パターンに一致するか照合してから考える
2. **座標・Transform の推測計算禁止** — 実際の値は MCP ツール（`get_gameobject`）またはシーンスナップショットで読む
3. **2回試みて直らない場合は停止** — 推測を続けず、現状をユーザーに報告して指示を仰ぐ

## シーンスナップショット

作業前に `EscapeGame/Dump Scene Snapshot` を実行すると `.claude/scene-snapshot.md` が更新される。
Transform・コンポーネント接続の確認はこのファイルを読む（MCP探索不要）。

## 先祖返り対応フロー

ユーザーから「変わっていない」「反映されていない」「戻っている」などの指摘があった場合のみ、以下を実行する：

1. mcp-unity の `save_scene` を実行（ユーザーの手動調整をディスクに保存）
2. `EscapeGame/Dump Scene Snapshot` を実行
3. snapshot.md と現状を照合してから修正に着手

通常作業ではこのフローはスキップして良い（毎回やるとトークン消費が大きい）。

## Transform 調整の方針

- **カメラポイント** (AltarPoint, PortraitPoint 等): MCP `set_transform` または Align With View で調整。Build Room2 Scene 実行で先祖返りしない（`EnsureCameraPoint` は既存があれば触らない）
- **ジオメトリ** (祭壇/食器棚/肖像画など): 必ず Editor 定数を書き換えて Build Room2 Scene で反映。MCP で動かすと再生成時に消える
- **初期値に戻したい時**: `EscapeGame/Reset Camera Point/{name}` メニュー（個別 / All）
- 詳細は [.claude/unity-patterns.md](.claude/unity-patterns.md) の「Transform 調整の使い分け」を参照

## Git運用（先祖返り対策）

- **破壊的Editorメニュー実行前は必ずコミット** — `Build Room2 Scene` 等は既存オブジェクトを破壊して再生成するため、コミット直後に実行する
- **GitGuard が自動チェック** — `Assets/_Project/Editor/GitGuard.cs` が `git status` を実行し、未コミット変更があればダイアログで警告
- **推奨フロー**:
  1. ターミナルで `git status` で変更確認
  2. `git add -A && git commit -m "進捗メッセージ"`
  3. Editor メニュー実行
  4. 結果が悪ければ `git restore .` で巻き戻し
- **マイルストーン到達時** — `git tag v0.X-room2complete` などでマーク
- **`.claude/scene-snapshot.md` は無視対象** — 自動生成されるためコミット不要（patterns.md は版管理する）

## パターンファイル（必読）

Unity コードを書く・Editor スクリプトを実行する前に必ず読むこと：
**[.claude/unity-patterns.md](.claude/unity-patterns.md)**

NG パターンを踏んだ場合・新しい OK パターンを発見した場合は、作業完了前にこのファイルへ即追記する。

## タスク種別ごとの完了条件

**コンパイル通過は完了ではない。** 以下の条件を満たしてから完了と報告する。

### UI実装（ボタン・パネル・オーバーレイ）
- Edit Mode でコンパイルエラーなし
- Play Mode でパネル表示・非表示が正常
- ボタンの onClick が発火する（コンソールログで確認）
- コンソールに Error/Warning がない
- Exit Play Mode 後も設定が残っている（先祖返りなし）

### インタラクタブル（クリック操作）
- Input System 経由で発火している（`OnMouseDown()` は使わない）
- 全画面オーバーレイがある場合は Input System で直接検知しているか確認
- Play Mode で実際にクリックして発火をコンソールログで目視確認

### パズルロジック（入力・判定・演出）
- 正解フロー：正解入力 → 演出 → 次状態へ遷移
- 不正解フロー：不正解入力 → 何も起きないまたは不正解演出
- コンソールに Error がない

### Editor スクリプト（セットアップ・ビルドスクリプト）
- 1回目実行：正常動作
- 2回目実行（冪等性）：エラーなし・重複なし

### セーブ/ロード
- セーブ実行 → Exit Play Mode → Enter Play Mode → ロード確認
- 新規追加アイテムが SaveManager.allItems に含まれているか確認

## 名前 / アセットパスの参照ルール

- **GameObject 名は `SceneNames.cs`、アセットパスは `AssetPaths.cs` の const 経由**で参照する（`Assets/_Project/Scripts/Core/`）
- コード内に文字列リテラルを直書きしない（`GameObject.Find("Canvas_Main")` 等は禁止）
- 新規 Find / LoadAssetAtPath を追加する時は、先に SceneNames / AssetPaths に const を追加してから参照
- シーン GameObject 名は日本語、コードの const 名は英語（例: `SceneNames.CanvasMain = "メインキャンバス"`）
- 例外: 動的命名テンプレ (`$"Candle_{i}_Stick"`) は `SceneNames.CandleStickFormat` の format 文字列で持つ
- 例外: フォントアセット (`NotoSansJP_Fresh.asset`) は TMP 動的アトラスの都合で英数字維持

## OnMouseDown / 3D クリック検知の注意

- **OnMouseDown は UI Raycast を貫通する** — UI ボタンの背後にある 3D Collider が誤発火する
  - 暫定対処: `if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;` を OnMouseDown 冒頭に
  - 本来の対処: `IPointerClickHandler` + `Physics Raycaster` に移行（設計改善案 C、未着手）
- **MonoBehaviour.enabled = false では OnMouseDown を必ずしも止められない** — 同一 GameObject 上の Collider 自体を `enabled = false` にする、もしくはコンポーネントごと削除する
- **複数の Collider があるオブジェクトに注意** — BoxCollider だけ無効化しても MeshCollider 経由で発火する（過去 SG_Canvas で事故）
- **Overview 中はエリア内オブジェクトを反応させない** — Interactable と `HoverHighlight` の両方で `RoomViewController.Instance.IsOverview` チェックを入れる

## シーン YAML 直接編集の注意

`.unity` ファイルを直接編集する場合（重複名 GameObject の選別削除など、MCP では難しいとき）：

- **コンポーネント削除は m_Component リスト除去 + 定義ブロック完全削除の両方が必須**。定義だけ残すと Unity がロード時に自動再付与する
- **fileID の型を必ず確認**してから削除（`!u!23` = MeshRenderer、`!u!64` = MeshCollider 等。詳細は [.claude/unity-patterns.md](.claude/unity-patterns.md) の「シーン YAML 直接編集の安全手順」）
- 編集後の Unity ダイアログでは **必ず Reload を選ぶ**（Ignore すると次の save_scene で編集が消える）

## 大型未使用アセットの退避

- `Library/Artifacts` が肥大化したら、まず `Assets/` 配下の大型外部 Asset Pack（Furniture Mega Pack 等）を疑う
- 退避手順は [.claude/unity-patterns.md](.claude/unity-patterns.md) の「大型未使用アセットの管理」を参照
- 退避先は `/Users/ohori/Documents/Claude/EscapeGame_Archive/`
