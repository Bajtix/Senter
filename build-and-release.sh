#!/bin/sh


echo "What is the release tag?"
read tag

git add .
git commit -S -m "release commit $tag"
git push

dotnet publish -c Release --os win --sc -o Build/win/
dotnet publish -c Release --os linux --sc -o Build/lin/
dotnet publish -c Release --os osx --sc -o Build/mac/

cd Build

zip -r windows.zip win/
zip -r linux.zip lin/
zip -r osx.zip mac/



gh release create $tag

gh release upload $tag windows.zip --clobber
gh release upload $tag linux.zip --clobber
gh release upload $tag osx.zip --clobber
