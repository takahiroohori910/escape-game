# 嵐の洋館 BGM/SE 生成指示書（Gemini Audio向け）

## 生成後の配置先
`Assets/_Project/Audio/` フォルダに配置する。
- BGM: `Assets/_Project/Audio/BGM/`
- SE:  `Assets/_Project/Audio/SE/`

---

## BGM

### BGM_Main.wav
- **用途**: ゲーム中ずっとループ再生するメインBGM
- **長さ**: 60〜90秒（完全ループ可能なこと）
- **雰囲気**: 暗くミステリアスな洋館。外では嵐が吹き荒れている。不安感・緊迫感・孤独感。ピアノ主体でストリングスが絡む。
- **テンポ**: ゆっくり (BPM 60〜75)
- **Prompt例**:  
  > "Dark mysterious piano ambience for an old haunted mansion escape room. Slow tempo, minor key, distant thunder and rain in the background, strings occasionally swelling with tension. Seamless loop, 70 seconds."

### BGM_Clear.wav
- **用途**: ゲームクリア時に流れるファンファーレ→余韻BGM
- **長さ**: 15〜20秒（ループ不要）
- **雰囲気**: 達成感・解放感。嵐が晴れていくような明るさ。ピアノ→オーケストラへ展開。
- **Prompt例**:  
  > "Short triumphant orchestral fanfare for an escape room clear screen. Starts with single piano notes then builds to a warm orchestral swell. 15 seconds, uplifting and emotional."

---

## SE（効果音）

### SE_Click.wav
- **用途**: 一般的なUIボタンクリック時（数字パッドのボタンなど）
- **長さ**: 0.1〜0.2秒
- **雰囲気**: 木や金属を軽く叩いたような短いクリック音。古い洋館に合うアナログ感。
- **Prompt例**:  
  > "Short wooden click sound effect, like pressing an old mechanical button. 0.15 seconds."

### SE_Hover.wav
- **用途**: インタラクタブルオブジェクトにカーソルが重なったとき
- **長さ**: 0.1秒以内
- **雰囲気**: 非常に短く繊細なチリン音またはペーパーノイズ
- **Prompt例**:  
  > "Very short subtle hover sound, like a faint paper rustle or soft chime tick. Under 0.1 seconds."

### SE_BookMove.wav
- **用途**: 本棚の本をクリックして動かしたとき
- **長さ**: 0.3〜0.5秒
- **雰囲気**: 本が棚をすべる音。紙と木がこすれるような音。
- **Prompt例**:  
  > "Sound of a book sliding on a wooden shelf. Soft scraping paper and wood sound, 0.4 seconds."

### SE_PuzzleSolve.wav
- **用途**: パズル（本棚・デスク）が正解したとき
- **長さ**: 0.8〜1.2秒
- **雰囲気**: 達成感のある上昇音。鍵が開くような金属音+短いチャイム。
- **Prompt例**:  
  > "Puzzle solved sound effect for an escape room. A satisfying click of a lock opening followed by a short ascending chime. 1 second."

### SE_PuzzleFail.wav
- **用途**: 暗証番号が不正解だったとき
- **長さ**: 0.5秒
- **雰囲気**: 軽いブザーまたは低音の「ぶっ」という音。怖すぎない程度。
- **Prompt例**:  
  > "Soft error buzz sound for wrong code entry in an escape room. Short and not too harsh, 0.5 seconds."

### SE_ItemPickup.wav
- **用途**: インベントリにアイテムが追加されたとき
- **長さ**: 0.4〜0.6秒
- **雰囲気**: アイテムを拾い上げる音。布や紙をつかむような柔らかい音+小さなチリン。
- **Prompt例**:  
  > "Item pickup sound for adventure game. Soft rustling sound followed by a gentle chime, like picking up a piece of equipment. 0.5 seconds."

### SE_CameraMove.wav
- **用途**: エリア間のカメラ移動開始時
- **長さ**: 0.3秒
- **雰囲気**: 重い靴で床板を踏む音または古い椅子の軋み音
- **Prompt例**:  
  > "Old wooden floor creaking sound, like footsteps in a haunted house. Single step, 0.3 seconds."

### SE_NoteOpen.wav
- **用途**: メモ・日記を開いたとき
- **長さ**: 0.3〜0.5秒
- **雰囲気**: 古い紙をめくる音
- **Prompt例**:  
  > "Sound of opening and unfolding an old paper note. Crisp paper rustling sound, 0.4 seconds."

### SE_PhoneRepair.wav
- **用途**: 電話の修理が完了したとき
- **長さ**: 0.8〜1.0秒
- **雰囲気**: 機械部品がカチッとはまる音+小さな電気音
- **Prompt例**:  
  > "Mechanical click sound of a component being fixed into place, followed by a brief electrical hum. 0.9 seconds, like repairing an old telephone."

### SE_PhoneCall.wav
- **用途**: 電話で救助を呼んだとき（ゲームクリア直前）
- **長さ**: 1.5〜2.0秒
- **雰囲気**: 古い電話のコール音（ジリリ…）が1〜2回鳴る
- **Prompt例**:  
  > "Old rotary telephone ringing sound. Two short rings, like an antique telephone. 1.8 seconds."

---

## 技術仕様
- **フォーマット**: WAV（16bit, 44100Hz）
- **BGMはステレオ**、**SEはモノラル**（Unity内でPan設定するため）
- ファイル名は上記の通り（`SE_Click.wav` など）

---

## Unity組み込み手順（後半作業で実施）
1. 上記ファイルを `Assets/_Project/Audio/BGM/` および `Assets/_Project/Audio/SE/` に配置
2. `EscapeGame/Setup/Phase2 Back Setup` を実行（後半セットアップで作成予定）
