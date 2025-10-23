using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

#region .: Criando Provider :.
Console.WriteLine("###                   Evento de Domínio.                   ###");
Console.WriteLine(new string('-', 80));
Console.WriteLine();

var services = new ServiceCollection();
services.AddScoped<IDomainEventHandler<FaturaConciliadaEvent>, AtualizarSatatusNotaHandler>();
services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
var serviceProvider = services.BuildServiceProvider();
#endregion


var fatura = new Fatura(1, 2, StatusConciliacao.EmAberto);

// Atualizando Status Conciliação Fatura
fatura.AtualizarStatusConciliacao(StatusConciliacao.PagamentoParcial);

var dispatcher = new DomainEventDispatcher(serviceProvider);
await dispatcher.PublicarAsync(fatura.Eventos);

Console.ReadKey();

#region .: Classes Evento :.
public interface IDomainEvent
{
    DateTime DataOcorrencia { get; }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent dominioEvento);
}

public class FaturaConciliadaEvent : IDomainEvent
{
    public int FaturaId { get; set; }
    public int NotaId { get; set; }
    public StatusConciliacao StatusConciliacao { get; set; }
    public DateTime DataOcorrencia => DateTime.UtcNow;

    public FaturaConciliadaEvent(int faturaId, int notaId, StatusConciliacao statusConciliacao)
    {
        FaturaId = faturaId;
        NotaId = notaId;
        StatusConciliacao = statusConciliacao;
    }
}

public class AtualizarSatatusNotaHandler : IDomainEventHandler<FaturaConciliadaEvent>
{
    private readonly INotaFiscalRepository _notaFiscalRepository;

    public AtualizarSatatusNotaHandler(INotaFiscalRepository notaFiscalRepository)
    {
        _notaFiscalRepository = notaFiscalRepository;
    }

    public async Task HandleAsync(FaturaConciliadaEvent dominioEvento)
    {
        var nota = _notaFiscalRepository.ObterPorId(dominioEvento.NotaId);
        Console.WriteLine($"[Nota Fiscal] #Buscando Nota Handler.");

        if (nota is null)
            return;

        nota.AtualizarStatusConciliacao(dominioEvento.StatusConciliacao);
        await _notaFiscalRepository.AtualizarAsync(nota);
    }
}

public class DomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, MethodInfo> _handleMetodos = [];

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublicarAsync(IEnumerable<IDomainEvent> eventos)
    {
        Console.WriteLine($"[Publicando Eventos] #Dispachante de Eventos");
        Console.WriteLine();
        foreach (var evento in eventos)
        {
            var handlerTipo = typeof(IDomainEventHandler<>).MakeGenericType(evento.GetType());
            var handlers = _serviceProvider.GetServices(handlerTipo);

            foreach (var handler in handlers)
            {
                var handleMetodo = _handleMetodos.GetOrAdd(handlerTipo, tipo => 
                                    tipo.GetMethod("HandleAsync") ?? 
                                    throw new InvalidOperationException($"Método HandleAsync não encontrado. {tipo}"));

                await (Task)handleMetodo.Invoke(handler, [evento])!;
            }
        }
    }
}
#endregion

#region .: Entidades Etc.. :.
public class Fatura
{
    public int Id { get; set; }
    public int NotaId { get; set; }
    public StatusConciliacao StatusConciliacao { get; set; }

    private readonly List<IDomainEvent> _eventos = [];
    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();


    public Fatura(int id, int notaId, StatusConciliacao statusConciliacao)
    {
        Id = id;
        NotaId = notaId;
        StatusConciliacao = statusConciliacao;
    }

    public void AtualizarStatusConciliacao(StatusConciliacao novoStatusConciliacao)
    {
        if (StatusConciliacao == StatusConciliacao.Pago)
            return;

        Console.WriteLine($"[Fatura] #Atualizando Status Conciliação da Fatura....");
        StatusConciliacao = novoStatusConciliacao;

        Console.WriteLine();
        Console.WriteLine($"[Fatura] #Criando Evento....");
        var evento = new FaturaConciliadaEvent(Id, NotaId, StatusConciliacao);

        Console.WriteLine();
        _eventos.Add(evento);
    }

    public void LimparEventos() => _eventos.Clear();
}

public class NotaFiscal(int Id, StatusConciliacao StatusConciliacao)
{
    public int Id { get; set; } = Id;
    public StatusConciliacao StatusConciliacao { get; set; } = StatusConciliacao;

    public void AtualizarStatusConciliacao(StatusConciliacao novoStatusConciliacao)
        => StatusConciliacao = novoStatusConciliacao;
}

public interface INotaFiscalRepository
{
    Task AtualizarAsync(NotaFiscal notaFiscal);
    NotaFiscal ObterPorId(int id);
}

public class NotaFiscalRepository : INotaFiscalRepository
{
    public Task AtualizarAsync(NotaFiscal notaFiscal)
    {
        Console.WriteLine();
        Console.WriteLine($"[Nota Repository] #Atualizando NotaId: {notaFiscal.Id} - novo Status: {notaFiscal.StatusConciliacao}");
        return Task.CompletedTask;
    }

    public NotaFiscal ObterPorId(int id)
    {
        return new NotaFiscal(id, StatusConciliacao.EmAberto);
    }
}

public enum StatusConciliacao
{
    EmAberto = 1,
    PagamentoParcial = 2,
    Pago = 3
}
#endregion

