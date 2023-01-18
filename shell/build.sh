#!/bin/bash
# 2021.3.12f1
 
UNITY_VERSION=$1
PACKAGENAME=$2
VERSION=$3
UNITY_PATH=/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity
PROJECT_PATH=/Users/jamisliao/Unity/Flow-unity-sdk
NOW=$(date +%F_%H-%M-%S)
BUILD_LOG_PATH=${PROJECT_PATH}/Logs/build_$NOW.log

echo Now: ${NOW}
echo Unity Version: ${UNITY_VERSION}
echo Unity Application Path: ${UNITY_PATH}
echo Package Name: ${PACKAGENAME}
echo Build Version: ${VERSION}
echo Project Path: ${PROJECT_PATH}
echo Log Path: ${BUILD_LOG_PATH}

$UNITY_PATH -quit -batchmode -projectPath $PROJECT_PATH -executeMethod Editor.BuildTool.Build -logFile $BUILD_LOG_PATH -action exportpackage -packageName $PACKAGENAME -buildVersion $VERSION

result=$?
echo ${result}

if [ ${result} -ne 0 ]
then
    echo 'Unity execute error.'
    exit 1
else
    echo 'complete export package'
    git checkout release/export_package
    git add release/${VERSION}/Portto.Blocto.${PACKAGENAME}.${VERSION}.unitypackage
    git commit -m "chore: add Portto.Blocto."${PACKAGENAME}"."${VERSION}".unitypackage"
    git push origin release/export_package
    git checkout develop
fi
