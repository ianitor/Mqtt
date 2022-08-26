
# Install Operator

´´´
helm install emqx-operator emqx/emqx-operator --set installCRDs=true --namespace emqx-operator-system --create-namespace
´´´

## check

´´´
kubectl get pods -l "control-plane=controller-manager" -n emqx-operator-system
´´´

## install broker

´´´
kubectl apply -f ./broker.yaml  
´´´

## check clustr status

´´´
kubectl exec -it emqx-0 -n emqx-operator-system -- emqx_ctl status
´´´