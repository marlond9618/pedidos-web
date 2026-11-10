namespace PedidosWeb.Core.Clientes;

/// <summary>Clientes de ejemplo (<see cref="ClientesDeEjemplo"/>). En la Unidad 5 se reemplaza por PostgreSQL.</summary>
public sealed class ClienteRepositorioEnMemoria : IClienteRepositorio
{
    private readonly IReadOnlyList<Cliente> _clientes;

    public ClienteRepositorioEnMemoria() : this(ClientesDeEjemplo.Crear()) { }

    public ClienteRepositorioEnMemoria(IReadOnlyList<Cliente> clientes)
    {
        _clientes = clientes;
    }

    public Cliente? ObtenerPorId(string id) =>
        _clientes.FirstOrDefault(c => c.Id == id);
}
