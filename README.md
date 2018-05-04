# Angle3DMonitor

- M5Stackによる傾斜計とスマートフォンでの傾斜モニタ（iOS/Android）。

# 必要なもの
- M5Stack Gray（9軸センサー搭載）
- Arduino IDE
  - バージョン1.8.5での動作を確認しています。
- VisualStudio for Mac
  - Windows用でも大丈夫だと思いますが、未確認です。
- スマートフォン
  - 下記の機種で動作確認しています。
    - iPhone7（iOS）
    - iPad mini 4（iOS）
    - Nexus5（Android）

# 使用しているOSSライブラリ
  - M5Stack（ https://github.com/m5stack/M5Stack ）
  - ESP32 BLE Arduino（ https://github.com/nkolban/ESP32_BLE_Arduino ）
    - M5Stack（ESP32）によるBLE通信用ライブラリ
  - UrhoSharp.Forms（ https://www.nuget.org/packages/UrhoSharp.Forms/ ）
    - Xamarin.Formsで利用可能な3D表示ライブラリ
  - Plugin.BLE( https://www.nuget.org/packages/Plugin.BLE/ )
    - Xamarinで利用可能なBLE通信ライブラリ

# ビルド
1. M5Stack
    1. PCにM5StackをUSB接続します。
    1. Arduino IDE（別途ダウンロードしてください）より、M5Stack/AngleMeter/AngleMeter.inoを開きます。
    1. 同IDE上で、「マイコンボードに書き込む」を実行します。
        - あらかじめ、下記の設定を行ってください。
          1. ボードは、「M5Stack-Core-ESP32」を指定。
          1. シリアルポートは、M5StackをUSB接続して追加されるCOMポートを指定。
              - COMポートがない場合、ドライバ（ https://www.silabs.com/products/development-tools/software/usb-to-uart-bridge-vcp-drivers ）のインストールが必要になります。
1. スマートフォン
    - iOSの場合
        1. VisualStudioで、ソリューションAngle3Dmonitor.slnを開きます。
        1. VisualStudio上で、Angle3DMonitor.iOSを「スタートアッププロジェクトとして設定」します。
        1. 実機（iPhone7など）をMacにUSB接続し、「デバッグの開始」を実行します。
            - 当方は、Apple Developer Programに登録しておらず、iOSによる実機デバッグにおいて、多少の手順が必要になります。（ここでは説明を省略しますが、「ios実機デバッグ　無料」などでググってください。）
    - Androidの場合
        1. VisualStudioで、ソリューションAngle3Dmonitor.slnを開きます。
        1. VisualStudio上で、Angle3DMonitor.Droidを「スタートアッププロジェクトとして設定」します。
        1. 実機（Nexus5など）をMacにUSB接続し、「デバッグの開始」を実行します。
