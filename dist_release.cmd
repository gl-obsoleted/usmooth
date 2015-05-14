set target_dir=usmooth_test_unity\Assets\usmooth\
set target_editor_dir=%target_dir%Editor

if not exist %target_editor_dir% mkdir %target_editor_dir%

copy usmooth\usmooth.runtimeplugin\bin\Release\ulog.dll %target_dir% /y
copy usmooth\usmooth.runtimeplugin\bin\Release\usmooth.dll %target_dir% /y
copy usmooth\usmooth.editorplugin\bin\Release\usmooth.Editor.dll %target_editor_dir% /y

