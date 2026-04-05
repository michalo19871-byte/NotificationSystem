
rm -rf NotificationSystem/.git

git fetch --unshallow

git pull https://$AZUSERNAME:$AZUREPAT@dev.azure.com/$AZORG/NotificationSystem/_git/NotificationSystem.git

git config --global user.name "$AZUSERNAME"
git config --global user.email "$AZUSER_EMAIL"

git add .
git commit -m "Automated commit from Azure DevOps pipeline"
git push --force https://$AZUSERNAME:$AZUREPAT@dev.azure.com/$AZORG/NotificationSystem/_git/NotificationSystem.git