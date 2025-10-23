
#region .: Console Exemplo 1 :.
//Console.WriteLine($"******* Exemplo 1 Pattern Observer *********");
//Console.WriteLine(new string('-', 90));
//Console.WriteLine();

//var fatura = new Fatura(1, 100m, Status.EmAberto);
//var nota = new NotaFisca();

//fatura.Registrar(nota);
//Console.WriteLine();

//Console.WriteLine($"Fatura-> {(string)fatura}");

//Console.WriteLine(new string('-', 90));

//fatura.AtualizarStatus(Status.Paga);

//Console.WriteLine(new string('-', 90));
#endregion

#region .: Console Exemplo 2 :.
var bolsa = new BolsaDeValores(100.0m);

var investidor1 = new Investidor("João");
var investidor2 = new Investidor("Maria");
var corretora = new Corretora();

bolsa.Registrar(investidor1);
bolsa.Registrar(corretora);
bolsa.Registrar(investidor2);

bolsa.MudarPreco(102.5m);
bolsa.MudarPreco(99.8m);

bolsa.Remover(investidor2);

bolsa.MudarPreco(101.0m);
#endregion


Console.ReadLine();

#region .: Classes Exemplo 1 :.
public class NotaFisca : IObserver<Fatura>
{
    public int NotaId { get; set; } = 1;
    public Status Status { get; set; }

    public NotaFisca() { }

    public NotaFisca(int notaId, Status status)
    {
        NotaId = notaId;
        Status = status;
    }

    public void AtualizarStatus(Status novoStatus) => Status = novoStatus;

    public void Update(Fatura evento)
    {
        if (evento is Fatura)
            AtualizarStatus(evento.Status);
        Console.WriteLine($"Nota Fiscal: {NotaId + evento.Id}  - Nota Status: {Status} - atualizada com sucesso......");
    }
}

public class Fatura : ISubject<Fatura>
{
    private readonly List<IObserver<Fatura>> _observadores = [];

    public int Id { get; set; }
    public decimal Valor { get; set; }
    public Status Status { get; set; }

    public Fatura(int faturaId, decimal valor, Status status)
    {

        Id = faturaId;
        Valor = valor;
        Status = status;
    }


    public void Registrar(IObserver<Fatura> observadores)
    {
        Console.WriteLine($"*** Observador registrado com sucesso. ***.");
        _observadores.Add(observadores);
    }

    public void AtualizarStatus(Status novoStatus)
    {
        if (Status != novoStatus)
        {
            Status = novoStatus;
            Notificar();
        }
    }

    public void Notificar()
    {
        foreach (var observer in _observadores)
            observer.Update(this);
    }

    public void Remover(IObserver<Fatura> observer)
    {
        Console.WriteLine($"Observador Removido.");
        _observadores.Remove(observer);
    }

    public static implicit operator string(Fatura fatura)
        => $@"FaturaId: {fatura.Id} - Valor: {fatura.Valor} - Status: {fatura.Status}";
}

public interface IObserver<T>
{
    void Update(T sujeito);
}

public interface ISubject<T>
{
    void Registrar(IObserver<T> observador);
    void Notificar();
    void Remover(IObserver<T> observador);
}

public enum Status
{
    EmAberto = 1,
    Paga = 2,
    Cancelado = 3
}
#endregion


#region .: Classe Exemplo 2 :.
public class BolsaDeValores : ISujeito
{
    private readonly List<IObservador> _observadores = [];
    public decimal PrecoAcao { get; private set; }

    public BolsaDeValores(decimal precoAcao)
    {
        PrecoAcao = precoAcao;
    }

    public void MudarPreco(decimal novoPreco)
    {
        if (PrecoAcao != novoPreco)
        {
            PrecoAcao = novoPreco;
            Console.WriteLine($"\nBolsa: O preço da ação mudou para {novoPreco:C}.");
            Notificar();
        }
    }

    public void Notificar()
    {
        foreach (var observador in _observadores)
        {
            observador.Atualizar(this);
        }
    }

    public void Registrar(IObservador observador)
    {
        Console.WriteLine($"Observador anexado.");
        _observadores.Add(observador);
    }

    public void Remover(IObservador observador)
    {
        Console.WriteLine($"Observador desanexado.");
        _observadores.Remove(observador);
    }
}

public class Investidor : IObservador
{
    private string _nome;

    public Investidor(string nome)
    {
        _nome = nome;
    }

    public void Atualizar(ISujeito sujeito)
    {
        if (sujeito is BolsaDeValores bolsa)
        {
            Console.WriteLine($"Investidor {_nome}: Notificado! O novo preço da ação é {bolsa.PrecoAcao:C}.");
        }
    }
}

public class Corretora : IObservador
{
    public void Atualizar(ISujeito sujeito)
    {
        if (sujeito is BolsaDeValores bolsa)
        {
            Console.WriteLine($"Corretora: Atualizando dados... O preço atual é {bolsa.PrecoAcao:C}.");
        }
    }
}

public interface ISujeito
{
    void Registrar(IObservador observador);
    void Notificar();
    void Remover(IObservador observador);
}

public interface IObservador
{
    void Atualizar(ISujeito sujeito);
}
#endregion
