Write-Host "Starting pod."
./removesub.ps1
kubectl apply -f ./mqtt_bench_sub.yaml

Write-Host "Waiting for pod ready."
kubectl wait --for=condition=Ready pod/csharpmqttbench-sub -n csharpmqttbench

Write-Host "Log output:"
kubectl -n mqtt-bench logs csharpmqttbench-sub -f

