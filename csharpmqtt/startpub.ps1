Write-Host "Starting deployment."
./removepub.ps1
kubectl apply -f ./mqtt_bench_pub2.yaml

Write-Host "Waiting for deployment ready."
kubectl wait --for=condition=Available deployment/csharpmqttbench-pub-deployment -n csharpmqttbench

#Write-Host "Log output:"
#kubectl -n mqtt-bench logs mqtt-bench-pub -f

