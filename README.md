# USBAuth - 通过 DigiSpark 实现 Windows 免密登录。

在新版的 Win10 中，微软添加了 `Windows Hello 人脸`、 `Windows Hello 指纹`、`安全密钥` 等登陆方式。对于不搭载专属配件的设备来说，实现任意一种免密登录的成本都比较高。而 `DigiSpark` 成本非常低，不到7元包邮，可以实现模拟键鼠。受`USB免密登录`的启发，通过 `DigiSpark` 实现 Windows 免密登录，可以做到 `移动密钥` 的效果 —— 插入即可免密登录，拔出则自动锁定设备。

![DigiSpark 正面](https://cdn.wyr.me/imgs/01.jpg)

![DigiSpark 背面](https://cdn.wyr.me/imgs/02.jpg)

烧录到DigiSpark的代码：
```C
#include "DigiKeyboard.h" //library declaration

void setup() {
  pinMode(1, OUTPUT); //LED on Model USB type A
  DigiKeyboard.update(); //Get the Keboard input ready
  DigiKeyboard.delay(500);
  DigiKeyboard.sendKeyStroke(KEY_SPACE);
  DigiKeyboard.delay(2000);
  DigiKeyboard.println("你的开机密码或Pin密码");
  DigiKeyboard.delay(500);
  DigiKeyboard.sendKeyStroke(KEY_ENTER);
  digitalWrite(1, HIGH); //Turn on the LED when program finishes
}

void loop() {
}
```


C# 实现 `拔出设备则自动锁定` 的 Windows服务源代码：
[https://github.com/yi-ge/USBAuth](https://github.com/yi-ge/USBAuth)

运行方法：
进入`bin\Release`，右键**以管理员权限**运行`USBAuth.exe`。

![拔出设备则自动锁定](https://cdn.wyr.me/imgs/03.jpg)

注意：添加设备期间，请勿激活命令行窗口（点击其他应用即可），否则设备执行回车将导致程序异常退出。

Blog: [https://www.wyr.me/post/613](https://www.wyr.me/post/613)
