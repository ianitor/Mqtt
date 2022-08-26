
# install

```
helm repo add vernemq https://vernemq.github.io/docker-vernemq
helm repo update
helm install vernemq vernemq/vernemq --namespace vernemq --create-namespace -f ./vernemq-values.yaml
```

Info EULA License must be accepted - set as environment variable at vernemq-values.yaml

# uninstall

```
helm uninstall vernemq -n vernemq
```