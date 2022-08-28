
# Install Operator

```
helm install emqx-operator emqx/emqx-operator --set installCRDs=true --namespace emqx-operator-system --create-namespace
```

## check

```
kubectl get pods -l "control-plane=controller-manager" -n emqx-operator-system
```

## install broker

```
kubectl apply -f ./broker.yaml  
```

## check status & cluster status

```
kubectl exec -it emqx-0 -n emqx-operator-system -- emqx_ctl status
kubectl exec -it emqx-0 -n emqx-operator-system -- emqx_ctl cluster status
```

cluster status should be for 3 nods:

```
Cluster status: #{running_nodes =>
['emqx@emqx-0.emqx-headless.emqx-operator-system.svc.cluster.local',
'emqx@emqx-1.emqx-headless.emqx-operator-system.svc.cluster.local',
'emqx@emqx-2.emqx-headless.emqx-operator-system.svc.cluster.local'],
stopped_nodes => []}
```