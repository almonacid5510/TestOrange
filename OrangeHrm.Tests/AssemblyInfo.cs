using Xunit;

// Configuración de ejecución en paralelo para exactamente 2 hilos (1 para Chrome y 1 para Edge)
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, MaxParallelThreads = 2)]
