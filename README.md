# 媒体播放器项目说明 / Media Player Project Description

## 项目简介 / Project Introduction

这是一个基于WPF+VLC开发的媒体播放器应用程序，支持视频文件播放，具有现代化的用户界面和丰富的功能特性。
This is a media player application developed based on WPF+VLC, supporting video file playback with a modern user interface and rich feature set.

## 主要功能 / Main Features

- **媒体播放 / Media Playback**：支持常见视频格式（MP4、AVI、MKV、WMV等）
- **播放控制 / Playback Controls**：播放/暂停、音量调节、进度控制
- **全屏功能 / Fullscreen Feature**：支持进入/退出全屏模式，ESC键快速退出全屏
- **控制栏自动隐藏 / Auto-hide Controls**：全屏模式下3秒无操作后控制栏自动隐藏，鼠标移动恢复显示
- **播放列表 / Playlist**：可从文件夹加载媒体文件列表
- **历史记录 / Playback History**：自动保存播放历史，按日期分类显示
- **主题切换 / Theme Switching**：支持多种预定义主题色和调色盘
- **键盘快捷键 / Keyboard Shortcuts**：提供常用功能的快捷键操作，包括ESC键退出全屏
- **侧边栏 / Sidebar**：可折叠/展开的侧边栏，显示历史记录和播放列表

## 项目结构 / Project Structure

```
Player/
├── App.xaml                     # 应用程序入口 / Application Entry Point
├── MainWindow.xaml              # 主窗口 / Main Window
├── Player.csproj                # 项目文件 / Project File
├── Behaviors/                  # 行为定义 / Behaviors Definition
│   └── VlcInitializationBehavior.cs
├── Bottom/                     # 底部控件（播放控制条）/ Bottom Control (Playback Control Bar)
│   ├── BottomControl.xaml
│   ├── BottomControl.xaml.cs
│   ├── BoolToFullscreenIconConverter.cs
│   ├── BoolToLeftColumnWidthConverter.cs
│   ├── BoolToPlayPauseIconConverter.cs
│   ├── BoolToWindowStyleConverter.cs
│   └── VolumeToIconConverter.cs
├── Core/                       # 旧版核心功能（已迁移）/ Legacy Core (Migrated)
│   └── (empty)
├── Error_notebook/             # 问题记录本 / Issue Notebook
│   ├── 2024-01-18_视频播放卡死问题分析.md
│   ├── 2025-11-02_ViewModel通信问题分析.md
│   ├── 2025-11-03_音量滑轨和按钮图标功能实现分析.md
│   ├── 2025-11-05_进度条拖拽不跟手问题分析.md
│   └── 2025-11-07_视频全屏播放问题解决方案总结.md
├── Fullscreen/                 # 全屏相关转换器 / Fullscreen Related Converters
│   ├── BoolToPlayPauseIconConverter.cs
│   └── VolumeToIconConverter.cs
├── Helpers/                    # 辅助类 / Helper Classes
│   ├── ColorToBrushConverter.cs
│   ├── CustomDialog.cs
│   ├── CustomIcon.cs
│   ├── LoadConfigManager.cs
│   ├── SpeedConverter.cs
│   ├── SystemNotificationHelper.cs
│   ├── ThemeManager.cs
│   └── VisualTreeHelperExtensions.cs
├── Image/                      # 图片资源 / Image Resources
│   ├── 初始状态.png
│   ├── 全屏悬浮控制栏.png
│   ├── 控制栏隐藏.png
│   ├── 文件列表折叠.png
│   ├── 视频选项.png
│   └── 调色盘.png
├── JSON/                       # 配置文件目录 / Configuration Files Directory
│   ├── hardware.json
│   ├── history.json
│   ├── settings.json
│   └── theme.json
├── Left/                       # 左侧控件（历史记录和播放列表）/ Left Controls (History and Playlist)
│   ├── BoolToIconConverter.cs
│   ├── BoolToTooltipConverter.cs
│   ├── BoolToWidthConverter.cs
│   ├── LeftControl.xaml
│   ├── LeftControl.xaml.cs
│   ├── SettingsDialog.xaml
│   └── SettingsDialog.xaml.cs
├── Middle/                     # 中间控件（视频播放区域）/ Middle Control (Video Playback Area)
│   ├── BoolToCornerRadiusConverter.cs
│   ├── BoolToFullscreenIconConverter.cs
│   ├── BoolToPlayPauseIconConverter.cs
│   ├── MiddleControl.xaml
│   ├── MiddleControl.xaml.cs
│   ├── TimeSpanToSliderValueConverter.cs
│   └── VolumeConverter.cs
├── Services/                    # 服务层 / Service Layer
│   ├── DependencyInjectionService.cs
│   ├── DialogService.cs
│   ├── DisposableManager.cs
│   ├── IDialogService.cs
│   ├── INavigationService.cs
│   ├── IThemeService.cs
│   ├── NavigationService.cs
│   ├── ServiceLocator.cs
│   ├── ThemeService.cs
│   ├── ViewModelLocator.cs
│   └── WpfNotificationService.cs
├── Themes/                     # 主题资源 / Theme Resources
│   ├── BlueTheme.xaml
│   ├── CustomStyles.xaml
│   ├── GreenTheme.xaml
│   ├── OrangeTheme.xaml
│   ├── PurpleTheme.xaml
│   ├── RedTheme.xaml
│   └── SettingsDialogStyles.xaml
└── ViewModels/                 # 数据模型 / Data Models
    ├── BottomViewModel.cs
    ├── IViewModelBase.cs
    ├── LeftViewModel.cs
    ├── MainViewModel.cs
    ├── MiddleViewModel.cs
    └── SettingsViewModel.cs

Player.Core/                        # 核心模块 / Core Module
├── Player.Core.csproj               # 核心项目文件 / Core Project File
├── Commands/                       # 命令定义 / Command Definitions
│   └── RelayCommand.cs
├── Enums/                         # 枚举定义 / Enumerations
│   └── IconKind.cs
├── Events/                        # 事件定义 / Event Definitions
│   ├── MediaMessage.cs
│   └── MockMediaPlayerEventArgs.cs
├── Helpers/                       # 核心辅助类 / Core Helper Classes
│   ├── CustomIcon.cs
│   └── UIControlManager.cs
├── JSON/                          # 配置文件目录 / Configuration Files Directory
│   ├── hardware.json
│   ├── settings.json
│   └── theme.json
├── Messaging/                     # 消息传递 / Messaging System
│   └── MessengerBase.cs
├── Models/                        # 数据模型 / Data Models
│   ├── MediaItem.cs
│   ├── PlaybackState.cs
│   ├── SettingPath.cs
│   └── Settings.cs
├── Repositories/                  # 仓储层 / Repository Layer
│   ├── IMediaRepository.cs
│   └── MediaRepository.cs
├── Services/                      # 核心服务层 / Core Service Layer
│   ├── ConfigManager.cs
│   ├── CoreThemeService.cs
│   ├── FileHistoryManager.cs
│   ├── IHistoryManager.cs
│   ├── IMessengerService.cs
│   ├── INotificationService.cs
│   ├── IThemeService.cs
│   ├── IViewModelBase.cs
│   ├── IVlcPlayerService.cs
│   ├── MessengerService.cs
│   ├── MockVlcPlayerService.cs
│   └── VlcPlayerService.cs
└── ViewModels/                    # 核心视图模型 / Core ViewModels
    └── ViewModelBase.cs
```

## 技术特点 / Technical Features

1. **MVVM架构 / MVVM Architecture**：采用Model-View-ViewModel设计模式
2. **响应式UI / Responsive UI**：基于WPF的数据绑定机制实现界面响应
3. **事件驱动 / Event-Driven**：使用事件机制实现组件间通信
4. **配置持久化 / Configuration Persistence**：使用JSON文件保存用户配置和历史记录
5. **错误处理 / Error Handling**：完善的异常处理和日志记录机制
6. **代码优化 / Code Optimization**：优化代码结构，解决高耦合低内聚问题，提升代码可维护性
7. **资源管理 / Resource Management**：完善的资源释放机制，避免内存泄漏
8. **接口实现 / Interface Implementation**：实现IDisposable接口，确保资源正确清理
9. **依赖注入 / Dependency Injection**：使用Microsoft.Extensions.DependencyInjection框架
10. **消息通信 / Message Communication**：实现组件间解耦的消息传递机制
11. **动画效果 / Animation Effects**：使用WPF动画系统实现平滑的UI过渡效果
12. **分层架构 / Layered Architecture**：清晰的项目结构，包含核心层、服务层、仓储层等

## 关键组件说明 / Key Component Descriptions

### MainViewModel
- 管理应用程序主窗口状态
- 协调不同ViewModel之间的通信
- 处理文件选择和窗口控制命令

### MiddleViewModel
- 管理VLC播放器的核心逻辑
- 处理播放控制命令（播放/暂停/停止）
- 实现全屏模式下的控制栏自动隐藏功能
- 管理共享的PlaybackState状态

### BottomViewModel
- 管理底部控制栏的用户交互
- 处理进度条、音量控制等功能
- 提供播放速度调节功能

### LeftViewModel
- 管理历史记录和播放列表
- 处理文件选择和加载功能
- 实现设置对话框的管理

### Player.Core 层
- 提供核心业务逻辑和数据模型
- 实现VLC播放器服务接口
- 提供消息传递机制和事件管理
- 实现文件历史和配置管理

### LeftControl
- 显示历史记录日期列表
- 展示对应日期的媒体文件列表
- 实现侧边栏折叠/展开功能
- 提供设置入口

### MiddleControl
- 负责媒体播放核心功能
- 处理全屏模式切换和视频渲染
- 实现高效的视频播放控制和状态管理
- 优化的资源释放机制，防止内存泄漏
- 平滑的尺寸变化和过渡效果处理

### BottomControl
- 提供播放控制栏
- 实现进度条、音量控制等交互功能
- 支持控制栏自动隐藏和显示功能

## 键盘快捷键 / Keyboard Shortcuts

- **空格键 / Space**：播放/暂停
- **Ctrl+O**：打开媒体文件
- **Ctrl+F**：切换全屏
- **ESC**：退出全屏模式
- **Ctrl+左箭头 / Ctrl+Left Arrow**：快退
- **Ctrl+右箭头 / Ctrl+Right Arrow**：快进
- **Ctrl+上箭头 / Ctrl+Up Arrow**：增加音量
- **Ctrl+下箭头 / Ctrl+Down Arrow**：减少音量
- **Ctrl+M**：静音/取消静音
- **Ctrl+B**：折叠/展开侧边栏

## 功能改进日志 / Feature Improvement Log

### 最新更新 / Recent Updates
- 2025-11-09 : 实现全屏模式下控制栏自动隐藏和显示功能，3秒无操作后自动隐藏，鼠标移动恢复显示
- 2025-11-09 : 使用平滑的透明度动画效果，提升用户体验
- 2025-11-09 : 实现控制栏透明度从1.0到0.01的平滑过渡
- 2025-10-31 : 优化全屏模式下控制栏交互体验，改进显示/隐藏逻辑
- 2025-10-31 : 实现ESC键快速退出全屏功能
- 2025-10-31 : 优化控制栏透明度设置，提升用户体验
- 2025-10-31 : 完善事件订阅管理，避免内存泄漏 
- 2025-11-01 : 建立统一的播放状态管理，防止icon与视频播放状态不符
- 2025-11-01 : 优化进度条平移的平滑度

### 已知问题 / Known Issues
1. 当视频播放结束后拖动进度条无法重播
2. 全屏状态下快速点击播放/暂停或者多次拖动进度条，会出画面撕裂卡顿

## 应用截图 / Application Screenshots

以下是播放器的主要界面截图：
The following are the main interface screenshots of the player:

### 初始状态 / Initial State
![初始状态](./Player/Image/初始状态.png)

### 全屏悬浮控制栏 / Fullscreen Floating Control Bar
![全屏悬浮控制栏](./Player/Image/全屏悬浮控制栏.png)

### 控制栏隐藏状态 / Control Bar Hidden State
![控制栏隐藏](./Player/Image/控制栏隐藏.png)

### 文件列表折叠 / File List Collapsed
![文件列表折叠](./Player/Image/文件列表折叠.png)

### 视频选项 / Video Options
![视频选项](./Player/Image/视频选项.png)

### 调色盘 / Color Palette
![调色盘](./Player/Image/调色盘.png)

## 系统要求 / System Requirements

- Windows系统 / Windows System
- .NET 8.0运行时 / .NET 8.0 Runtime
- VLC媒体播放器核心组件（libvlc）/ VLC Media Player Core Components (libvlc)

## 开源协议 / Open Source License

本项目基于 GNU Lesser General Public License v2.1 or later (LGPL-2.1-or-later) 许可。
This project is licensed under the GNU Lesser General Public License v2.1 or later (LGPL-2.1-or-later).

## 第三方库致谢 / Third-Party Libraries Acknowledgments

本项目使用了多个开源库，详细信息请参阅 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) 文件。
This project uses multiple open-source libraries, for detailed information please refer to the [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) file.

在此，我们向所有开源作者表示诚挚的感谢和敬意！
Here, we express our sincere gratitude and respect to all open-source authors!

## 开发环境 / Development Environment

- Visual Studio 2022+
- Visual Studio Code
- .NET 8.0 SDK
- WPF开发工具包 / WPF Development Toolkit
- VLC媒体播放器开发库 / VLC Media Player Development Library