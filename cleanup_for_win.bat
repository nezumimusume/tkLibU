@chcp 65001
@echo on
@rem クリーンアップ
@setlocal
@pushd tkLibU_URP

@rmdir /s /q Assets
@rmdir /s /q Assets
@del Assets
@del Assets
@popd


@pushd tkLibU_BRP
@rmdir /s /q Assets
@rmdir /s /q Assets
@del Assets
@del Assets
@popd

@echo クリーンアップ終わり
@pause