kubectl apply -f configmap.yaml
kubectl apply -f secret.yaml
kubectl apply -f dockerconfigsecret.yaml
kubectl apply -f postgres-init-configmap.yaml

kubectl apply -f postgres.yaml

kubectl apply -f kafka/kafka.yaml

kubectl apply -f monolith.yaml

kubectl apply -f movies-service.yaml
kubectl apply -f events-service.yaml

kubectl apply -f proxy-service.yaml

echo any key...
pause
