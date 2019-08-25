def exec(cmd) {
    if (isUnix()) {
        sh cmd
    }
    else {
        bat cmd
    }
}

node {
    stage('Git pull') {
        checkout scm
    }
    stage('Pack nuget') {
        exec('dotnet pack src/DeOlho.EventBus.Message -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus.Message/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917 || true')
        exec('dotnet pack src/DeOlho.EventBus -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917')
        exec('dotnet pack src/DeOlho.EventBus.MediatR -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus.MediatR/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917')
        exec('dotnet pack src/DeOlho.EventBus.EventSourcing -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus.EventSourcing/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917')
        exec('dotnet pack src/DeOlho.EventBus.RabbitMQ -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus.RabbitMQ/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917')
        exec('dotnet pack src/DeOlho.EventBus.RabbitMQ.DependencyInjection -c Release')
        exec('dotnet nuget push src/DeOlho.EventBus.RabbitMQ.DependencyInjection/bin/Release/ -s DeOlho -k 730ebfc8d61bea02ac6a5262c8cca917')
    }
}