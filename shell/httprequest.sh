  #!/bin/bash

PACKAGENAME=$1
BUILDVERSION=$2

NAME="EXPORT_PACKAGENAME"
REPO="jamis-portto/unity_projectsettings_myself"
URL="https://api.github.com/repos/$REPO/actions/variables"
UPDATEURL_PACKAGENAME="https://api.github.com/repos/$REPO/actions/variables/$NAME"
UPDATEURL_BUILDVERSION="https://api.github.com/repos/$REPO/actions/variables/EXPORT_BUILDVERSION"

response=$(curl -X PATCH -H "Accept: application/vnd.github+json" -H "Authorization: token ${GIT_TOKEN}" -H "X-GitHub-Api-Version: 2022-11-28" \
                -d '{"name":"EXPORT_PACKAGENAME","value":"'${PACKAGENAME}'"}' \
                -s -w "%{http_code}" $UPDATEURL_PACKAGENAME )

http_code=$(tail -n1 <<< "$response")  # get the last line
content=$(sed '$ d' <<< "$response")   # get all but the last line which contains the status code

if [ $http_code -eq 204 ]
then
    echo "updated successed"
else
    exit 1
fi

response=$(curl -X PATCH -H "Accept: application/vnd.github+json" -H "Authorization: token ${GIT_TOKEN}" -H "X-GitHub-Api-Version: 2022-11-28" \
                -d '{"name":"EXPORT_BUILDVERSION","value":"'${BUILDVERSION}'"}' \
                -s -w "%{http_code}" $UPDATEURL_BUILDVERSION )

http_code=$(tail -n1 <<< "$response")  # get the last line
content=$(sed '$ d' <<< "$response")   # get all but the last line which contains the status code

if [ $http_code -eq 204 ]
then
    echo "updated successed"
else
    exit 1
fi

echo $content
echo "$http_code"
echo "$content"