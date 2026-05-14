# Unity 開発パターン集（EscapeGame）

Unity 6 (6000.4.3f1) / URP / macOS 環境での実績ベース。
NGパターンを踏んだときは即このファイルに追記すること。

---

## NG → OK 対応表

### 3D オブジェクト生成（Editor スクリプト）

| NG | OK | 理由 |
|---|---|---|
| `Resources.GetBuiltinResource<Mesh>("Cube.fbx")` | `GameObject.CreatePrimitive(PrimitiveType.Cube)` | Unity 6 では GetBuiltinResource が機能しない |
| `AddComponent<MeshFilter>().sharedMesh = null` | CreatePrimitive を使えばMeshFilterとMeshが最初から入っている | MeshRenderer が null 参照エラーを起こす |

### BoxCollider の操作（Editor スクリプト）

| NG | OK | 理由 |
|---|---|---|
| `DestroyImmediate(col); go.AddComponent<BoxCollider>()` | `var col = go.GetComponent<BoxCollider>(); if (col == null) col = go.AddComponent<BoxCollider>(); if (col != null) col.size = size;` | DestroyImmediate 後の AddComponent が null を返すことがある（Unity 6 バグ） |

### Runtime Event の Editor 時配線

| NG | OK | 理由 |
|---|---|---|
| `button.onClick.AddListener(method)` をEditor スクリプトで呼ぶ | `UnityEventTools.AddPersistentListener(event, method)` | AddListener はシリアライズされずシーン保存後に消える |
| ラムダ式を AddPersistentListener に渡す | メソッド参照のみ渡す | ラムダはシリアライズ不可 |

### Private フィールドへの値セット（Editor スクリプト）

| NG | OK | 理由 |
|---|---|---|
| SerializedField に直接代入 | リフレクション `GetField(name, NonPublic \| Instance).SetValue(obj, val)` または `SerializedObject + FindProperty + ApplyModifiedProperties` | Editor スクリプトから private フィールドへの直接アクセス不可 |
| リフレクション（ScriptableObject の private フィールド） | `SerializedObject` の方が安全 | ただし両方使える。ScriptableObjectにはリフレクション、MonoBehaviourにはSerializedObjectが多い |

### コンポーネント追加順序

| NG | OK | 理由 |
|---|---|---|
| `[RequireComponent(typeof(Button))]` なクラスを先にAddComponent | Button を先にAddComponent してからターゲットクラスを追加 | 依存コンポーネントが自動追加される保証はEditorスクリプトでは薄い |

### アセンブリ・コンパイル

| NG | OK | 理由 |
|---|---|---|
| 新規 .cs ファイル作成直後に即 Recompile | `Assets/Refresh` → Recompile の順 | ファイルが Unity に認識される前にコンパイルが走るとクラスが見つからない |

### Unity 6 API

| NG（旧API） | OK（Unity 6） | 理由 |
|---|---|---|
| `FindObjectOfType<T>()` | `FindAnyObjectByType<T>()` | Unity 6 で deprecated |
| `FindObjectsByType<T>(FindObjectsSortMode)` | `FindObjectsByType<T>()` | FindObjectsSortMode 版が deprecated |
| `TMP_Text.enableWordWrapping` | `TMP_Text.textWrappingMode` | Unity 6 で deprecated |

---

## 推奨パターン集

### Editor スクリプトの冪等性（何度実行しても壊れない）

```csharp
// GameObjectは Find で探して、なければ作る
static GameObject EnsureGO(string name, Transform parent) {
    var go = GameObject.Find(name);
    if (go == null) { go = new GameObject(name); if (parent != null) go.transform.SetParent(parent); }
    return go;
}

// コンポーネントも GetComponent→AddComponent パターン
var comp = go.GetComponent<MyComponent>() ?? go.AddComponent<MyComponent>();
```

### ScriptableObject をコードで作成する

```csharp
string path = "Assets/_Project/ScriptableObjects/Notes/MyNote.asset";
var asset = AssetDatabase.LoadAssetAtPath<NoteData>(path);
if (asset == null) {
    asset = ScriptableObject.CreateInstance<NoteData>();
    AssetDatabase.CreateAsset(asset, path);
}
// private フィールドはリフレクションで
var f = asset.GetType().GetField("title", BindingFlags.NonPublic | BindingFlags.Instance);
f?.SetValue(asset, "タイトル");
EditorUtility.SetDirty(asset);
AssetDatabase.SaveAssets();
```

### Canvas UI を Editor スクリプトで構築する

```csharp
// 1. RectTransform を持つ GameObject を作る
var go = new GameObject(name);
go.transform.SetParent(parent.transform, false);
var rt = go.AddComponent<RectTransform>();
rt.sizeDelta = new Vector2(width, height);

// 2. アンカー設定
rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // 中央
rt.anchoredPosition = Vector2.zero;

// 3. Image（先） → Button（後）の順でAddComponent
go.AddComponent<Image>().color = color;
go.AddComponent<Button>();
```

### MonoBehaviour の SerializedField を Editor から設定する

```csharp
var so = new SerializedObject(component);
so.FindProperty("fieldName").objectReferenceValue = someObject;
so.FindProperty("stringField").stringValue = "value";
so.FindProperty("intField").intValue = 42;
so.ApplyModifiedProperties();
EditorUtility.SetDirty(component);
```

---

## Room2 固有の設計メモ

- Room2 はワールド座標 x=50 にオフセット（Room1 は x=0 付近）
- カメラポイント名: Overview2Point / StainedGlassPoint / DisplayCabinetPoint / CandelabraPoint / PortraitPoint / AltarPoint
- 正解コード: DisplayCabinet="2314"（薔薇2・十字3・星1・菱形4）、Altar="7429"
- Room2CodeButton が CurrentArea でどちらの UI に転送するか判定する

---

## Editor スクリプトで必ず作成が必要なコンポーネント

シーンに存在しないとランタイムで NPE クラッシュするため、
`FindAnyObjectByType` で見つからない場合は **必ず自分で作成する**。

| コンポーネント | 依存するUI | 作成場所 |
|---|---|---|
| DisplayCabinetPuzzle | DisplayCabinetUI.Awake() | Cab_Lock |
| AltarPuzzle | AltarUI.Awake() | Altar_Lock |
| PortraitPuzzle | PortraitSymbolInteractable | PortraitPuzzle GO |
| CandelabraPuzzle | CandleInteractable | CandelabraPuzzle GO |

→ WireRoom2UIs() で `FindAnyObjectByType ?? AddComponent` パターンを使う。

## SaveManager.allItems の更新を忘れずに

新しいアイテム（ItemData）を追加したら、SaveManager の `allItems` 配列にも追加しないとセーブ・ロードで消える。
Build スクリプトの WireRoom2UIs() で SerializedObject を使って追加する：

```csharp
var prop = saveSO.FindProperty("allItems");
bool hasKey = false;
for (int i = 0; i < prop.arraySize; i++)
    if (prop.GetArrayElementAtIndex(i).objectReferenceValue == itemRoomKey) { hasKey = true; break; }
if (!hasKey) {
    prop.InsertArrayElementAtIndex(prop.arraySize);
    prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = itemRoomKey;
    saveSO.ApplyModifiedProperties();
}
```

## foreach 中のコレクション変更は NG

```csharp
// NG: UseItem が内部リストを変更する
foreach (var item in InventoryManager.Instance.GetItems())
    if (item.ItemId == x) { InventoryManager.Instance.UseItem(item); break; }

// OK: 先に参照を取得してから変更
ItemData target = null;
foreach (var item in InventoryManager.Instance.GetItems())
    if (item.ItemId == x) { target = item; break; }
if (target != null) InventoryManager.Instance.UseItem(target);
```

## フォント適用の手順（文字化け発生時）

Edit Mode で以下を順番に実行する：
1. `EscapeGame/Setup/Create Japanese Font Asset` — フォントアセット生成
2. `EscapeGame/Setup/Apply Japanese Font to All TMP` — 全TMP対象に適用

### MenuItem のシグネチャ制約

| NG | OK | 理由 |
|---|---|---|
| `[MenuItem] public static void Foo(SomeType arg = null)` | `[MenuItem] public static void FooMenu()` → 内部で `Foo(null)` 呼ぶ | デフォルト引数があるとMenuItemとして認識されず実行エラーになる |
| `FindObjectsByType<T>(FindObjectsSortMode.None)` でフォント適用 | `FindObjectsByType<T>(FindObjectsInactive.Include)` | FindObjectsSortMode は Unity 6 で deprecated。inactive オブジェクト漏れ防止に Include 必須 |

## UI クリック遮断の回避

全画面オーバーレイ（NoteOverlay など Image + raycastTarget=true）が上に乗っているとき、下層の UI ボタンの onClick は**EventSystem に届かず発火しない**。

**回避策**: ボタンのスクリプトで `Mouse.current.leftButton.wasPressedThisFrame` を `Update()` で直接検知し、`RectTransformUtility.RectangleContainsScreenPoint` で自分のRect内かチェックする。Input System はレイキャストと独立して動作するため遮断されない。

```csharp
private void Update()
{
    if (Mouse.current == null) return;
    if (!Mouse.current.leftButton.wasPressedThisFrame) return;
    if (!IsPointerOver()) return;
    Execute();
}
```

## TextMeshProUGUI の事前描画（Dynamic フォント）

`FontWarmup` などで `TextMeshProUGUI` を `new GameObject` に追加して `ForceMeshUpdate()` しても、**Canvas 階層外では動作しない**（TMP UI は Canvas 必須）。

**OK パターン**: `FindAnyObjectByType<Canvas>()` で取得した Canvas の `transform` を親にしてから TMP を追加する。

```csharp
var canvas = FindAnyObjectByType<Canvas>();
var go = new GameObject("_Prewarm");
go.transform.SetParent(canvas.transform, false);
var t = go.AddComponent<TextMeshProUGUI>();
t.text = AllChars;
t.ForceMeshUpdate();
```

---

## Runtime NG→OK パターン

### クリック・タッチ検知

| NG | OK | 理由 |
|---|---|---|
| `void OnMouseDown()` | `Mouse.current.leftButton.wasPressedThisFrame` を `Update()` で検知 | 新Input System有効時、OnMouseDown()は発火しない |
| EventSystem経由のクリック（全画面オーバーレイ下） | Input System直接検知 + `RectTransformUtility.RectangleContainsScreenPoint` | raycastTarget=trueのオーバーレイがあるとEventSystemに届かない |

> CandleInteractable / PortraitSymbolInteractable / DisplayCabinetInteractable の3スクリプトで同じミスを繰り返した。新規インタラクタブルは必ず Input System パターンで実装する。

```csharp
private void Update()
{
    if (Mouse.current == null) return;
    if (!Mouse.current.leftButton.wasPressedThisFrame) return;
    if (!IsPointerOver()) return;
    Execute();
}

private bool IsPointerOver()
{
    var cam = Camera.main;
    return Physics.Raycast(cam.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hit)
        && hit.collider.gameObject == gameObject;
}
```

### コンポーネント参照・初期化順

| NG | OK | 理由 |
|---|---|---|
| `Awake()` で `FindAnyObjectByType<T>()` をそのまま使う | null のとき自分で作る / `Start()` に移す | Awake の実行順は保証されない |
| Play Mode 開始直後に参照をそのまま使う | null チェック必須 | 初期化順次第で null のまま |

---

## Transform 調整の使い分け（重要）

対象ごとに **どの手段で動かすか** が異なる。これを守らないと「Build Room2 Scene 実行で消える」現象が起きる。

| 対象 | 推奨手段 | 理由 |
|---|---|---|
| **CameraPoint** (AltarPoint, PortraitPoint 等) | **MCP set_transform** または **Align With View** | `EnsureCameraPoint` は案A（既存があれば触らない）。Build Room2 Scene 実行で先祖返りしない |
| **Geometry** (祭壇/食器棚/肖像画など) | **必ず Editor 定数を書き換えて Build Room2 Scene** | Build Room2 が既存を `DestroyImmediate` → 再生成するため、MCP で動かすと消える |

### 「やっぱり初期位置に戻したい」時

CameraPoint だけ `EscapeGame/Reset Camera Point/{name}` メニューで個別リセット可能：
- `EscapeGame/Reset Camera Point/All`
- `EscapeGame/Reset Camera Point/AltarPoint` 等

各メニューは GitGuard 経由で破壊的操作扱い（未コミット変更があれば警告）。

### NG パターン

- ❌ ジオメトリ（Cab_Body, Altar_Top 等）を MCP set_transform で動かす → Build Room2 Scene で消える
- ❌ CameraPoint の Editor 定数を書き換えてビルドしても、既存があれば反映されない（Reset メニュー使用）

## カメラポイント・オブジェクト配置の調整フロー

数値だけで調整すると繰り返し修正になる。必ず以下の順序を守る：

1. **Scene View でドラッグして目標位置に合わせる**（数値先行禁止）
2. Transform の数値を読み取り、下表に記録する
3. 記録した数値を Editor スクリプトに書き込む

### 確定座標メモ

| オブジェクト | Position | Rotation | 状態 |
|---|---|---|---|
| AltarPoint | (0, 2.92, 11.44) | (0, 0, 0) | 確定（Align View 経由） |

---

## 気づいたことはここに追記

<!-- 新しいNG/OKを見つけたらセクションに追加。日付も書くと振り返りやすい -->

---

## 【重要・絶対遵守】フォント関連メニューは原則使用禁止 (2026-05-14 追加)

過去にプロジェクト UI が全文字化け→約30分の復旧作業を要する事故が発生した。

### 禁止メニュー（叩くと壊れる）

| メニュー | 何が起こるか | 影響範囲 |
|---|---|---|
| `EscapeGame/Setup/Apply Japanese Font to All TMP` | 全 TMP を **NotoSansJP_Dynamic（壊れている方）に切替** | シーン全体の TMP |
| `EscapeGame/Font/Force Rebuild Atlas` | Dynamic の atlas texture を **破壊** | NotoSansJP_Dynamic.asset |
| `EscapeGame/Font/Restore NotoSansJP` | 全 TMP を Dynamic に再設定（Apply Japanese Font と同じ） | シーン全体 |

### 正しいフォント運用

- プロジェクトの**健全な日本語フォントは `NotoSansJP_Fresh.asset`** のみ（`Dynamic` は事実上壊れている）
- 新規 TMP を Editor スクリプトで作成する際は **必ず明示的にアサイン**：
  ```csharp
  var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansJP_Fresh.asset");
  if (jpFont != null) tmp.font = jpFont;
  ```
- フォント警告が出ても**メニューで一括解決しようとしない**。1個ずつ Editor スクリプトで font 代入する

### もし全文字化けが発生したら

1. `git restore Assets/_Project/Scenes/StudyRoom.unity` でシーンを最後のコミットへ戻す
2. `git restore Assets/_Project/Fonts/NotoSansJP_Dynamic.asset` で破損したアセットを戻す
3. Unity がダイアログ「modified externally」を出すので **Reload** を押す
4. 復旧確認

---

## Collider 2重問題：hit.collider 比較は危険 (2026-05-14 追加)

### 症状

`CreatePrimitive(Sphere)` 等で作ったオブジェクトに `AddBoxCollider` で BoxCollider を追加すると、**SphereCollider と BoxCollider が同居**する。Raycast クリック判定で：

```csharp
private void Awake() => col = GetComponent<Collider>();
// ...
if (hit.collider != col) return;  // ←NG！失敗する
```

`GetComponent<Collider>()` は先に追加された SphereCollider を返すが、`Physics.Raycast` は大きい方の BoxCollider にヒット → `hit.collider != col` で常に弾かれる。

### OK パターン

```csharp
if (hit.transform != transform) return;  // 同 GameObject かどうか transform で比較
```

GameObject 比較なら collider が複数あっても OK。`Awake` での `col` 取得も不要。

→ `CabinetCounterButton.cs`, `CabinetLever.cs` でこのパターンを採用。

---

## 親回転下の TextMeshPro 3D は読み方向が反転する (2026-05-14 追加)

### 症状

cabRoot (Y=90 回転) の子に TextMeshPro (3D) を配置すると、テキストが **右→左方向で描画され、カメラから見ると鏡文字**になる。

### 原因

TMP 3D のテキスト読み方向はローカル +X。親の Y=90 回転で local +X が world -Z にマップされ、カメラの「右」(world +Z) と逆向きになる。

### OK パターン

TMP の `localEulerAngles = (0, 180, 0)` を設定して、読み方向をカメラ視点で正しい向きに反転：

```csharp
var dispGO = new GameObject(name + "_Disp");
dispGO.transform.SetParent(parent.transform, false);
dispGO.transform.localEulerAngles = new Vector3(0f, 180f, 0f); // ← 親 Y=90 補正
var tmp = dispGO.AddComponent<TextMeshPro>();
```

→ 食器棚解錠装置のカウンタ表示・レバーラベルでこのパターンを採用。

---

## ItemDetailUI にはアイコン Image が必要 (2026-05-14 追加)

### 経緯

ItemDetailUI は当初「名前 + 説明」のみ表示で、ItemData.icon を**どこにも表示していなかった**。視覚的ヒントが必要なアイテム（凹凸パターンの蝋板など）が完全に意味を失う。

### 拡張パターン

- `ItemDetailUI` に `[SerializeField] private Image iconImage;` を追加
- `Show(ItemData)` で `iconImage.sprite = item.Icon` を設定
- パネルレイアウトを「名前（上）/ アイコン（中央、preserveAspect=true）/ 説明（下）」3段に
- 説明文が空でもアイコンだけで意味が伝わる

### 取得時の自動表示

パズル解錠で取得物を渡す側で `ItemDetailUI.Show()` を直接呼ぶ：

```csharp
InventoryManager.Instance?.AddItem(item);
FindAnyObjectByType<ItemDetailUI>()?.Show(item);
```

これでプレイヤーが「何を入手したか」を必ず目視できる。

---

## URP/Lit を加算ブレンドに切り替える（炎用） (2026-05-14 追加)

外側スフィア（不透明な URP/Lit）で内側の高輝度コアが隠れる問題。

### コードで設定する加算ブレンド

```csharp
static void ConfigureFlameMaterialAdditive(Material mat)
{
    mat.SetFloat("_Surface", 1f);                     // 1 = Transparent
    mat.SetFloat("_Blend", 2f);                        // 2 = Additive
    mat.SetFloat("_SrcBlend", (float)BlendMode.One);
    mat.SetFloat("_DstBlend", (float)BlendMode.One);
    mat.SetFloat("_ZWrite", 0f);
    mat.SetOverrideTag("RenderType", "Transparent");
    mat.renderQueue = 3000;
    mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    mat.DisableKeyword("_ALPHATEST_ON");
    EditorUtility.SetDirty(mat);
}
```

これで外側スフィアの色が「加算」され、奥にある内側コアの emission も画面に重なって光って見える。

---

## 【絶対遵守】指示外の変更を加えない (2026-05-14 追加)

> 「先祖返り（指示なしの変更）」は信頼を毀損する最大要因。

### 過去の事故例

- ユーザー指示: 「蝋燭は壁付け燭台の炎と同じ**光**を使う」
- 誤った対応: 「光」を「見た目全体」と拡大解釈し、内側コアスフィアを**勝手に削除**
- 結果: ユーザーの意図と逆方向の変更で激怒
- 教訓: 「光」=「Point Light のパラメータ」と最小限解釈すべきだった

### ルール

1. **指示された範囲のみ**変更する。拡大解釈しない
2. 過去ユーザーが確定させた状態（git にコミット済み）は**指示なく触らない**
3. 不明確な指示はその場で**選択肢を提示して確認**する
4. 「ついでに〇〇も直そう」は禁止。別タスクとして提案だけする

---

## 取得物のヒントを難しくしたい時のテンプレート (2026-05-14 追加)

食器棚→燭台パズルで採用したパターン（テキストヒントなし、視覚記号と現物の照合のみ）。

### 構成要素

1. **取得アイテム (ItemData)**: アイコンに「**抽象的な凹凸パターン**」だけ描く
   - 高い要素 = 目立つ色（赤）、低い要素 = 暗い色
   - 説明文は**空**にする
2. **ターゲット側オブジェクト (3D)**: アイコンと**同じパターンの物理的な高さ差**を付ける
   - 例: 蝋燭の高さを `0.28`（高） / `0.14`（低） に
   - 受け皿/土台の位置は変えず、蝋燭の底だけ揃える
3. **プレイヤーの導線**:
   - 取得 → アイコンを ItemDetailUI で**強制表示**して観察を促す
   - インベントリ再クリックでいつでも見返せる
   - 答えはアイコン + 現物の照合でしか得られない

### この方法の利点

- テキストヒントゼロ → 直接答えを書いていないので難度が高い
- 「アイコンと現物の対応」を発見する瞬間がエスケープゲーム的快感
- 別言語にも自然に対応（凹凸パターンは言語非依存）

