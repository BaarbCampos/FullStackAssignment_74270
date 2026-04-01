import { useEffect, useMemo, useState } from "react";

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

function getItemName(item) {
    return (
        item.productName ||
        item.name ||
        item.product?.name ||
        item.productTitle ||
        "Unknown product"
    );
}

function getItemQuantity(item) {
    return item.quantity ?? item.qty ?? 0;
}

function getItemPrice(item) {
    return item.price ?? item.unitPrice ?? item.product?.price ?? 0;
}

function OrdersPage() {
    const [orders, setOrders] = useState([]);
    const [error, setError] = useState("");
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [statusFilter, setStatusFilter] = useState("All");

    useEffect(() => {
        fetch("https://localhost:7045/api/orders")
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

    function handleViewDetails(order) {
        setSelectedOrder(order);

        setTimeout(() => {
            const panel = document.getElementById("order-details-panel");
            if (panel) {
                panel.scrollIntoView({ behavior: "smooth", block: "start" });
            }
        }, 100);
    }

    function handleCloseDetails() {
        setSelectedOrder(null);
    }

    const filteredOrders =
        statusFilter === "All"
            ? orders
            : orders.filter((order) => getStatusName(order.status) === statusFilter);

    const totalOrders = orders.length;

    const completedOrders = useMemo(() => {
        return orders.filter((order) => order.status === 10).length;
    }, [orders]);

    const failedOrders = useMemo(() => {
        return orders.filter((order) => order.status === 11).length;
    }, [orders]);

    const pendingOrders = useMemo(() => {
        return orders.filter((order) => ![10, 11].includes(order.status)).length;
    }, [orders]);

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
            <h1 style={{ marginBottom: "8px", fontSize: "38px" }}>
                📊 Admin Dashboard
            </h1>

            <p style={{ marginTop: 0, color: "#cbd5e1", marginBottom: "24px" }}>
                Manage orders and view details in one place.
            </p>

            {error && (
                <div
                    style={{
                        backgroundColor: "#3f1116",
                        border: "1px solid #7f1d1d",
                        color: "#fecaca",
                        padding: "12px 16px",
                        borderRadius: "10px",
                        marginBottom: "20px",
                        fontWeight: "bold"
                    }}
                >
                    {error}
                </div>
            )}

            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(4, minmax(180px, 1fr))",
                    gap: "16px",
                    marginBottom: "24px"
                }}
            >
                <div style={summaryCardStyle}>
                    <div style={summaryLabelStyle}>Total Orders</div>
                    <div style={summaryValueStyle}>{totalOrders}</div>
                </div>

                <div style={summaryCardStyle}>
                    <div style={summaryLabelStyle}>Completed</div>
                    <div style={{ ...summaryValueStyle, color: "#4ade80" }}>
                        {completedOrders}
                    </div>
                </div>

                <div style={summaryCardStyle}>
                    <div style={summaryLabelStyle}>Failed</div>
                    <div style={{ ...summaryValueStyle, color: "#f87171" }}>
                        {failedOrders}
                    </div>
                </div>

                <div style={summaryCardStyle}>
                    <div style={summaryLabelStyle}>Pending / Other</div>
                    <div style={{ ...summaryValueStyle, color: "#38bdf8" }}>
                        {pendingOrders}
                    </div>
                </div>
            </div>

            <div
                style={{
                    marginBottom: "20px",
                    padding: "18px",
                    backgroundColor: "#1e293b",
                    border: "1px solid #334155",
                    borderRadius: "14px",
                    display: "flex",
                    flexWrap: "wrap",
                    gap: "12px",
                    alignItems: "center",
                    justifyContent: "space-between"
                }}
            >
                <div style={{ display: "flex", alignItems: "center", gap: "12px", flexWrap: "wrap" }}>
                    <label htmlFor="statusFilter" style={{ fontWeight: "bold" }}>
                        Filter by status:
                    </label>

                    <select
                        id="statusFilter"
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                        style={selectStyle}
                    >
                        <option value="All">All</option>
                        <option value="Submitted">Submitted</option>
                        <option value="Inventory Pending">Inventory Pending</option>
                        <option value="Inventory Confirmed">Inventory Confirmed</option>
                        <option value="Inventory Failed">Inventory Failed</option>
                        <option value="Payment Pending">Payment Pending</option>
                        <option value="Payment Approved">Payment Approved</option>
                        <option value="Payment Failed">Payment Failed</option>
                        <option value="Shipping Pending">Shipping Pending</option>
                        <option value="Shipping Created">Shipping Created</option>
                        <option value="Completed">Completed</option>
                        <option value="Failed">Failed</option>
                    </select>

                    <button
                        onClick={() => setStatusFilter("All")}
                        style={clearButtonStyle}
                    >
                        Clear Filter
                    </button>
                </div>

                <div style={{ color: "#cbd5e1", fontWeight: "bold" }}>
                    Showing {filteredOrders.length} order(s)
                </div>
            </div>

            <div
                style={{
                    backgroundColor: "#111827",
                    border: "1px solid #334155",
                    borderRadius: "14px",
                    overflow: "hidden"
                }}
            >
                <table
                    style={{
                        width: "100%",
                        borderCollapse: "collapse"
                    }}
                >
                    <thead>
                        <tr style={{ backgroundColor: "#0b1220" }}>
                            <th style={thStyle}>ID</th>
                            <th style={thStyle}>Email</th>
                            <th style={thStyle}>Status</th>
                            <th style={thStyle}>Total</th>
                            <th style={thStyle}>Actions</th>
                        </tr>
                    </thead>

                    <tbody>
                        {filteredOrders.length > 0 ? (
                            filteredOrders.map((order) => (
                                <tr
                                    key={order.id}
                                    style={{
                                        backgroundColor:
                                            selectedOrder?.id === order.id ? "#1e293b" : "transparent"
                                    }}
                                >
                                    <td style={tdStyle}>{order.id}</td>
                                    <td style={tdStyle}>{order.customerEmail}</td>
                                    <td style={tdStyle}>
                                        <span
                                            style={{
                                                backgroundColor: getStatusColor(order.status),
                                                color: order.status === 5 || order.status === 8 ? "black" : "white",
                                                padding: "6px 10px",
                                                borderRadius: "999px",
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
                                            onClick={() => handleViewDetails(order)}
                                            style={buttonStyle}
                                        >
                                            View Details
                                        </button>
                                    </td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td style={tdStyle} colSpan="5">
                                    No orders found for this status.
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {!selectedOrder && (
                <p
                    style={{
                        marginTop: "20px",
                        color: "#cbd5e1",
                        fontSize: "16px"
                    }}
                >
                    Select an order to view details.
                </p>
            )}

            {selectedOrder && (
                <div
                    id="order-details-panel"
                    style={{
                        marginTop: "24px",
                        padding: "24px",
                        border: "1px solid #334155",
                        borderRadius: "14px",
                        backgroundColor: "#1e293b",
                        color: "white"
                    }}
                >
                    <h2 style={{ marginTop: 0, marginBottom: "18px" }}>Order Details</h2>

                    <div
                        style={{
                            display: "grid",
                            gridTemplateColumns: "repeat(2, minmax(260px, 1fr))",
                            gap: "16px",
                            marginBottom: "20px"
                        }}
                    >
                        <div style={detailCardStyle}>
                            <div style={detailLabelStyle}>Order ID</div>
                            <div style={detailValueStyle}>{selectedOrder.id}</div>
                        </div>

                        <div style={detailCardStyle}>
                            <div style={detailLabelStyle}>Email</div>
                            <div style={detailValueStyle}>{selectedOrder.customerEmail}</div>
                        </div>

                        <div style={detailCardStyle}>
                            <div style={detailLabelStyle}>Status</div>
                            <div style={{ marginTop: "8px" }}>
                                <span
                                    style={{
                                        backgroundColor: getStatusColor(selectedOrder.status),
                                        color:
                                            selectedOrder.status === 5 || selectedOrder.status === 8
                                                ? "black"
                                                : "white",
                                        padding: "6px 10px",
                                        borderRadius: "999px",
                                        fontWeight: "bold",
                                        display: "inline-block"
                                    }}
                                >
                                    {getStatusName(selectedOrder.status)}
                                </span>
                            </div>
                        </div>

                        <div style={detailCardStyle}>
                            <div style={detailLabelStyle}>Total</div>
                            <div style={detailValueStyle}>€{selectedOrder.totalAmount}</div>
                        </div>
                    </div>

                    <div style={{ marginTop: "8px" }}>
                        <h3 style={{ marginBottom: "12px" }}>Order Items</h3>

                        {selectedOrder.items && selectedOrder.items.length > 0 ? (
                            <div
                                style={{
                                    border: "1px solid #334155",
                                    borderRadius: "12px",
                                    overflow: "hidden"
                                }}
                            >
                                <table
                                    style={{
                                        width: "100%",
                                        borderCollapse: "collapse"
                                    }}
                                >
                                    <thead>
                                        <tr style={{ backgroundColor: "#0f172a" }}>
                                            <th style={thStyle}>Product</th>
                                            <th style={thStyle}>Quantity</th>
                                            <th style={thStyle}>Price</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {selectedOrder.items.map((item, index) => (
                                            <tr key={index}>
                                                <td style={tdStyle}>{getItemName(item)}</td>
                                                <td style={tdStyle}>{getItemQuantity(item)}</td>
                                                <td style={tdStyle}>€{getItemPrice(item)}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ) : (
                            <p style={{ color: "#cbd5e1" }}>No items found for this order.</p>
                        )}
                    </div>

                    <button
                        onClick={handleCloseDetails}
                        style={{
                            marginTop: "20px",
                            backgroundColor: "#dc3545",
                            color: "white",
                            border: "none",
                            padding: "10px 14px",
                            borderRadius: "10px",
                            fontWeight: "bold",
                            cursor: "pointer"
                        }}
                    >
                        Close
                    </button>
                </div>
            )}
        </div>
    );
}

const summaryCardStyle = {
    backgroundColor: "#1e293b",
    border: "1px solid #334155",
    borderRadius: "14px",
    padding: "18px"
};

const summaryLabelStyle = {
    color: "#cbd5e1",
    fontSize: "14px",
    marginBottom: "8px"
};

const summaryValueStyle = {
    fontSize: "28px",
    fontWeight: "bold",
    color: "white"
};

const detailCardStyle = {
    backgroundColor: "#0f172a",
    border: "1px solid #334155",
    borderRadius: "12px",
    padding: "14px"
};

const detailLabelStyle = {
    fontSize: "14px",
    color: "#94a3b8",
    marginBottom: "6px"
};

const detailValueStyle = {
    fontSize: "18px",
    fontWeight: "bold",
    wordBreak: "break-word"
};

const selectStyle = {
    padding: "9px 12px",
    borderRadius: "10px",
    border: "1px solid #475569",
    backgroundColor: "#0f172a",
    color: "white",
    fontWeight: "bold"
};

const clearButtonStyle = {
    backgroundColor: "#475569",
    color: "white",
    border: "none",
    padding: "9px 12px",
    borderRadius: "10px",
    fontWeight: "bold",
    cursor: "pointer"
};

const thStyle = {
    border: "1px solid #334155",
    padding: "14px",
    textAlign: "left"
};

const tdStyle = {
    border: "1px solid #334155",
    padding: "14px"
};

const buttonStyle = {
    backgroundColor: "#0dcaf0",
    color: "#000",
    border: "none",
    padding: "9px 12px",
    borderRadius: "10px",
    fontWeight: "bold",
    cursor: "pointer"
};

export default OrdersPage;