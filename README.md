# TestNetCore
编译说明：
1.最好是去官网  https://www.microsoft.com/net/core#windowscmd
  了解NetCore编译运行方法和下载对应平台的SDK。
2.NetCore2.0平台，IOCP通信模式，支持TCP和UDP，序列化方式为Protobuf和JSON。客户端为Unity2017.1.0。
3.消息协议改为注册ID读取配置文件，然后自动分发执行业务模式。可以注册和登录测试用。先不要重复登录。
4.KCP控制的UDP传输测试已经加入。KCP测试完成服务端分配对应的KCP对象，目前是一对一。
5.后面预计加入TCP和KCP控制的UDP协议的会话控制和多个连接对象的管理。
