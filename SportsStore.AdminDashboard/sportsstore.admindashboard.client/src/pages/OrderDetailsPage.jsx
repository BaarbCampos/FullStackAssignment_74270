import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

function getStatusName(status) {
  switch (status) {
    case 1:
      return "Submitted";
    case 2:
      return "Inventory Pending";
    case 3:
      return "Inventory Confirmed";
    case 4:
      return "Inventory Failed";
    case 5:
      return "Payment Pending";
    case 6:
      return "Payment Approved";
    case 7:
      return "Payment Failed";
    case 8:
      return "Shipping Pending";
    case 9:
      return "Shipping Created";
    case 10:
      return "Completed";
    case 11:
      return "Failed";
    default:
      return "Unknown";
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

function OrderDetailsPage() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`https://localhost:7040/api/orders/${id}`)
      .then((res) => {
        if (!res.ok) {
          throw new Error("Error fetching order details");
        }
        return res.json();
      })
      .then((data) => {
        setOrder(data);
      })
      .catch((err) => {
        console.error(err);
        setError("Could not load order details");
      })
      .finally(() => {
        setLoading(false);
      });
  }, [id]);

  if (loading) {
    return (
      <div style={pageStyle}>
        <p>Loading order details...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div style={pageStyle}>
        <p style={errorStyle}>{error}</p>
        <Link to="/" style={linkStyle}>
          ← Back to Orders
        </Link>
      </div>
    );
  }

  if (!order) {
    return (
      <div style={pageStyle}>
        <p>Order not found.</p>
        <Link to="/" style={linkStyle}>
          ← Back to Orders
        </Link>
      </div>
    );
  }

  return (
    <div style={pageStyle}>
      <Link to="/" style={linkStyle}>
        ← Back to Orders
      </Link>

      <h1 style={detailsTitleStyle}>Order #{order.id}</h1>

      <div style={cardStyle}>
        <p>
          <strong>Status:</strong>{" "}
          <span
            style={{
              backgroundColor: getStatusColor(order.status),
              color:
                order.status === 5 || order.status === 8
                  ? "black"
                  : "white",
              padding: "6px 10px",
              borderRadius: "12px",
              fontWeight: "bold",
              display: "inline-block"
            }}
          >
            {getStatusName(order.status)}
          </span>
        </p>

        <p>
          <strong>Email:</strong> {order.customerEmail}
        </p>

        <p>
          <strong>Total:</strong> €{order.totalAmount}
        </p>
      </div>

      <h2 style={sectionTitleStyle}>Order Items</h2>

      {order.items && order.items.length > 0 ? (
        <table style={tableStyle}>
          <thead>
            <tr style={headerRowStyle}>
              <th style={thStyle}>Product</th>
              <th style={thStyle}>Quantity</th>
              <th style={thStyle}>Unit Price</th>
              <th style={thStyle}>Subtotal</th>
            </tr>
          </thead>

          <tbody>
            {order.items.map((item, index) => (
              <tr key={index}>
                <td style={tdStyle}>{item.productName}</td>
                <td style={tdStyle}>{item.quantity}</td>
                <td style={tdStyle}>€{item.unitPrice}</td>
                <td style={tdStyle}>€{item.quantity * item.unitPrice}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p>No items found for this order.</p>
      )}
    </div>
  );
}

const pageStyle = {
  padding: "30px",
  fontFamily: "Arial, sans-serif",
  color: "white",
  minHeight: "100vh",
  backgroundColor: "#0f172a"
};

const detailsTitleStyle = {
  marginTop: "20px",
  marginBottom: "20px"
};

const sectionTitleStyle = {
  marginBottom: "15px"
};

const errorStyle = {
  color: "red",
  fontWeight: "bold"
};

const tableStyle = {
  width: "100%",
  borderCollapse: "collapse",
  marginTop: "10px"
};

const headerRowStyle = {
  backgroundColor: "#1f1f1f"
};

const thStyle = {
  border: "1px solid #444",
  padding: "12px",
  textAlign: "left"
};

const tdStyle = {
  border: "1px solid #444",
  padding: "12px"
};

const cardStyle = {
  backgroundColor: "#1f1f1f",
  padding: "20px",
  borderRadius: "10px",
  marginBottom: "25px",
  border: "1px solid #444"
};

const linkStyle = {
  color: "#0dcaf0",
  textDecoration: "none",
  fontWeight: "bold"
};

export default OrderDetailsPage;