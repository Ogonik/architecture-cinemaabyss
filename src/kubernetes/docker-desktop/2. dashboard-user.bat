kubectl apply -f admin-user_service-account.yaml
kubectl apply -f admin-user_cluster-role-binding.yaml

echo Any key...
pause

kubectl -n kubernetes-dashboard create token admin-user
echo Copy key...

pause