import { useEffect, useState } from "react";

function getStatusName(status) {
  switch (status) {
    case 1: return "Submitted";
    case 2: return "Inventory Pending";
    case 3: return "Inventory Confirmed";
    case 4: return "Inventory Failed";
    case 5: return "Payment Pending";
    case 6: return "Payment Approved";
    case 7: return "Payment Failed";
    case 8: return "Shipping Pending";
    case 9: return "Shipping Created";
    case 10: return "Completed";
    case 11: return "Failed";
    default: return "Unknown";
  }
}

function getStatusColor(status) {
  switch (status) {
    case 10:
      return "#198754";
    case 11:
      return "#dc3545";
    case 5:
    case 8:
      return "#ffc107";
    default:
      return "#0dcaf0";
  }
}

function OrdersPage() {
  const [orders, setOrders] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://localhost:7040/api/orders")
      .then((res) => {
        if (!res.ok) {
          throw new Error("Error fetching orders");
        }
        return res.json();
      })
      .then((data) => {
        setOrders(data);
      })
      .catch((err) => {
        console.error(err);
        setError("Could not load orders");
      });
  }, []);

  function handleViewDetails(orderId) {
    alert(`Order details for: ${orderId}`);
  }

  return (
    <div
      style={{
        padding: "30px",
        fontFamily: "Arial, sans-serif",
        color: "white",
        minHeight: "100vh",
        backgroundColor: "#0f172a"
      }}
    >
      <h1 style={{ marginBottom: "10px" }}>📊 Admin Dashboard</h1>

      {error && (
        <p style={{ color: "red", fontWeight: "bold" }}>
          {error}
        </p>
      )}

      <table
        style={{
          width: "100%",
          borderCollapse: "collapse",
          marginTop: "20px"
        }}
      >
        <thead>
          <tr style={{ backgroundColor: "#1f1f1f" }}>
            <th style={thStyle}>ID</th>
            <th style={thStyle}>Email</th>
            <th style={thStyle}>Status</th>
            <th style={thStyle}>Total</th>
            <th style={thStyle}>Actions</th>
          </tr>
        </thead>

        <tbody>
          {orders.map((order) => (
            <tr key={order.id}>
              <td style={tdStyle}>{order.id}</td>
              <td style={tdStyle}>{order.customerEmail}</td>
              <td style={tdStyle}>
                <span
                  style={{
                    backgroundColor: getStatusColor(order.status),
                    color: order.status === 5 || order.status === 8 ? "black" : "white",
                    padding: "6px 10px",
                    borderRadius: "12px",
                    fontWeight: "bold",
                    display: "inline-block"
                  }}
                >
                  {getStatusName(order.status)}
                </span>
              </td>
              <td style={tdStyle}>€{order.totalAmount}</td>
              <td style={tdStyle}>
                <button
                  onClick={() => handleViewDetails(order.id)}
                  style={buttonStyle}
                >
                  View Details
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

const thStyle = {
  border: "1px solid #444",
  padding: "12px",
  textAlign: "left"
};

const tdStyle = {
  border: "1px solid #444",
  padding: "12px"
};

const buttonStyle = {
  backgroundColor: "#0dcaf0",
  color: "#000",
  border: "none",
  padding: "8px 12px",
  borderRadius: "8px",
  fontWeight: "bold",
  cursor: "pointer"
};

export default OrdersPage;