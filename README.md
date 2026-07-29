# ☕ Cafetería Elay Puej - Despliegue en AWS EC2 y Contenerización Docker

Aplicación Web de Tres Capas (**Vue.js + ASP.NET Core 8.0 Web API + MariaDB/MySQL**) contenerizada con **Docker Compose** y desplegada en **AWS EC2**.

---

## 🚀 Arquitectura y Tecnologías
- **Frontend**: Vue.js 3 (Vite + SPA + Nginx) en puerto 80.
- **Backend**: ASP.NET Core 8.0 Web API en puerto 8080.
- **Base de Datos**: MariaDB / MySQL 10.11 en puerto 3306 (con volumen de datos persistente `mysql_data`).
- **Red Interna Docker**: `cafeteria_net`.

---

## 🛠️ Ejecución Local con Docker Compose

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/TU_USUARIO/CafeteriaElayPuej.git
   cd CafeteriaElayPuej
   ```

2. **Crear el archivo `.env`**:
   ```bash
   cp .env.example .env
   ```

3. **Construir e Iniciar los Servicios**:
   ```bash
   docker compose up -d --build
   ```

4. **Verificar Estado de los Contenedores**:
   ```bash
   docker compose ps
   ```

5. **Acceder a la Aplicación**:
   - Frontend: `http://localhost`
   - Backend Swagger API: `http://localhost:8080/swagger`

---

## 🌐 Guía de Despliegue en AWS EC2 (Fases 1 a 8)

### FASE 1: Revisión y Contenerización Local
El proyecto cuenta con `Dockerfile` en Frontend y Backend, `docker-compose.yml` para la orquestación y `.env.example` para las credenciales.

### FASE 2: Subir el Proyecto a GitHub
```bash
git init
git add .
git commit -m "feat: contenerizacion con docker y docker compose para AWS"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/CafeteriaElayPuej.git
git push -u origin main
```

### FASE 3: Crear Cuenta AWS
Inicia sesión en la [Consola de AWS](https://aws.amazon.com/) y selecciona la región `us-east-1` (N. Virginia) o la más cercana.

### FASE 4: Crear Servidor EC2
1. Ve a **EC2** -> **Launch Instance**.
2. **Nombre**: `cafeteria-elay-puej-server`.
3. **AMI**: Ubuntu Server 24.04 LTS (o 22.04 LTS).
4. **Instancia**: `t2.micro` o `t3.micro` (Free Tier).
5. **Key Pair**: Crear o seleccionar tu par de llaves (ej: `cafeteria-key.pem`).
6. **Security Group (Reglas de Entrada)**:
   - **SSH** (Puerto 22): `0.0.0.0/0`
   - **HTTP** (Puerto 80): `0.0.0.0/0`
   - **HTTPS** (Puerto 443): `0.0.0.0/0`

### FASE 5: Instalar Docker en la Instancia EC2
Conéctate por SSH a tu instancia:
```bash
chmod 400 cafeteria-key.pem
ssh -i cafeteria-key.pem ubuntu@IP_PUBLICA_EC2
```
Ejecuta los siguientes comandos para instalar Docker:
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y ca-certificates curl gnupg lsb-release

sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

sudo usermod -aG docker $USER
newgrp docker
```

### FASE 6: Desplegar la Aplicación en EC2
```bash
git clone https://github.com/TU_USUARIO/CafeteriaElayPuej.git
cd CafeteriaElayPuej
cp .env.example .env
docker compose up -d --build
```
Verifica que los 3 contenedores estén corriendo:
```bash
docker compose ps
```
Abre `http://IP_PUBLICA_EC2` en tu navegador.

### FASE 7: Configurar Dominio (Opcional/Recomendado)
Configura un dominio en DuckDNS, No-IP o Cloudflare agregando un registro **A** apuntando a tu `IP_PUBLICA_EC2`.

### FASE 8: Configurar HTTPS (SSL con Certbot)
```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d TU_DOMINIO.duckdns.org
```
¡Tu aplicación quedará desplegada con certificado SSL activo en `https://TU_DOMINIO.duckdns.org`!
