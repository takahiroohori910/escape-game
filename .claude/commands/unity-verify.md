# /unity-verify — Edit Mode 検証スキル

実装後、Play Mode に入る前に Edit Mode 側の問題を洗い出す。
視覚的・空間的問題（カメラ位置・オブジェクト配置）は対象外。目視確認は自分で行うこと。

## 実行手順

1. **コンソールログ確認**
   - `mcp__mcp-unity__get_console_logs` でエラー・警告を取得
   - Error があれば原因を特定して修正してから次へ進まない
   - Warning は内容を判断して重要なものは修正する

2. **対象 GameObjects の存在確認**
   - タスクで触った GameObject を `mcp__mcp-unity__get_gameobject` で取得
   - コンポーネントが正しく接続されているか確認
   - SerializedField の参照が null でないか確認

3. **シーン情報確認**
   - `mcp__mcp-unity__get_scene_info` で現在のシーン状態を確認
   - 想定外の GameObject が増減していないか確認

4. **確認結果を報告**
   以下の形式で報告する：
   ```
   ## /unity-verify 結果
   - コンソール: エラー0件 / 警告N件（内容）
   - GameObjects: OK / NG（詳細）
   - 判定: Play Mode テスト可 / 要修正（修正内容）
   ```

5. **Play Mode テスト**
   Edit Mode 検証がすべて OK なら、ユーザーに Play Mode テストを促す。
   空間的・視覚的確認（カメラ位置・オブジェクト配置）はユーザーが目視で行う。
