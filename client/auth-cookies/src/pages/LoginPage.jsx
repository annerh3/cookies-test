// components/LoginPage.jsx
import React, { useState } from "react";
import { useAuthStore } from "../store/useAuthStore";
import { useNavigate } from "react-router";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const { login, message, error } = useAuthStore();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    await login({ email, password });
    setLoading(false);

    if (!error) {
      navigate("/contacts"); // redirigir a la página de contactos
    } 
  };

  return (
    <div className="w-full flex justify-center">
      <div className="sm:min-w-96 mt-40 bg-coal shadow-lg p-6 rounded-lg">
        <h2 className=" font-bold mb-6 text-center text-white text-md sm:text-2xl">Iniciar Sesión</h2>
        <form onSubmit={handleSubmit} className="space-y-4" autoComplete="off">
          
          <div>
            <label className="block text-sm font-medium text-white">
              Correo
            </label>
            <input
              autoComplete="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 border border-white/50 rounded-lg mt-1 text-white"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-white">
              Contraseña
            </label>
            <input
              autoComplete="new-password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-4 py-2 border border-white/50 rounded-lg mt-1 text-white"
            />
          </div>
          {error && <p className="text-red-500 text-sm">{message || "Fallo en Inicio de sesión"}</p>}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-[#772CE8] cursor-pointer text-white py-2 rounded-lg hover:bg-[#743ec5] transition"
          >
            {loading ? "Cargando..." : "Iniciar Sesión"}
          </button>
        </form>
      </div>
    </div>
  );
}
