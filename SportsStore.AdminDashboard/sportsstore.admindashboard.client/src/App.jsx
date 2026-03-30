import { useEffect, useState } from "react";

function App() {
  const [orders, setOrders] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://localhost:7040/api/orders")
      .then(res => {
        if (!res.ok) throw new Error("Erro ao buscar pedidos");
        return res.json();
      })
      .then(data => setOrders(data))
      .catch(err => {
        console.error(err);
        setError("Não foi possível carregar os pedidos");
      });
  }, []);

  return (
    <div style={{ padding: "30px", fontFamily: "Arial" }}>
      <h1>📊 Admin Dashboard</h1>

      {error && <p style={{ color: "red" }}>{error}</p>}

      <table border="1" cellPadding="10" style={{ marginTop: "20px" }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Email</th>
            <th>Status</th>
            <th>Total</th>
          </tr>
        </thead>

        <tbody>
          {orders.map(o => (
            <tr key={o.id}>
              <td>{o.id}</td>
              <td>{o.customerEmail}</td>
              <td>{o.status}</td>
              <td>€{o.totalAmount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default App;