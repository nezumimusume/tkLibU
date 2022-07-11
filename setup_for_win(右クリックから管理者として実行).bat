@chcp 65001
@echo off
@rem サンプルの各フォルダにk2Engineへのシンボリックリンクを作成する。
@setlocal
@set CURRENT_DIR=%~dp0

@pushd %CURRENT_DIR%

@mklink /D "%CURRENT_DIR%\tkLibU_URP\Assets" "%CURRENT_DIR%\tkLibU_Core\Assets"
@mklink /D "%CURRENT_DIR%\tkLibU_BRP\Assets" "%CURRENT_DIR%\tkLibU_Core\Assets"

@popd

@echo セットアップ終了
@pause
