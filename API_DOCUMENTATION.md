--- 

*(Add more controllers and endpoints as they are developed)*

---

## Health Checks

- **Endpoint:** `GET /health`
- **Description:** Provides the health status of the API and its critical dependencies (e.g., database, message bus).
- **Request Body:** None
- **Success Response:**
    - Code: `200 OK`
    - Body: `application/json` (Provides a detailed breakdown of health check statuses. The structure can vary based on
      the checks configured but generally follows the HealthChecks.UI format.)
        ```json
        {
          "status": "Healthy", // Overall status: Healthy, Degraded, Unhealthy
          "totalDuration": "00:00:00.1234567",
          "entries": {
            "database": {
              "data": {},
              "description": null,
              "duration": "00:00:00.0123456",
              "status": "Healthy", // Status for this check
              "tags": []
            },
            "azure_service_bus_queue": {
              "data": {},
              "description": null,
              "duration": "00:00:00.0234567",
              "status": "Healthy",
              "tags": []
            }
            // ... more entries for other checks
          }
        }
        ```
- **Error Responses:**
    - Code: `503 Service Unavailable`: If one or more critical checks fail, the overall status might be Unhealthy, and
      the service might return 503. 