Write-Host "Starting pod."
./remove.ps1
kubectl apply -f ./stresser.yaml

Write-Host "Waiting for pod ready."
kubectl wait --for=condition=Ready pod/mqtt-stresser -n stresser

Write-Host "Log output:"
kubectl -n stresser logs mqtt-stresser -f

