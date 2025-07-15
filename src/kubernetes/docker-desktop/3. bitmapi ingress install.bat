helm upgrade --install cinemaabyss-ingress oci://registry-1.docker.io/bitnamicharts/nginx-ingress-controller --create-namespace --namespace cinemaabyss

kubectl -n cinemaabyss apply -f cinemaabyss-ingress.yaml