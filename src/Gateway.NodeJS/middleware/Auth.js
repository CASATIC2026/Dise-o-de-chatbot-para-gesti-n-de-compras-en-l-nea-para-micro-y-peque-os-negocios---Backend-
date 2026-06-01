/**
 * JWT Authentication Middleware
 */
import jwt from 'jsonwebtoken';

const JWT_SECRET = process.env.JWT_SECRET || 'your_super_secret_jwt_key_change_this_in_production';
export function authenticateToken(req, res, next) {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

    if (!token) {
        return res.status(401).json({ message: 'Access token required' });
    }

    try {
        const decoded = jwt.verify(token, JWT_SECRET);
        req.user = decoded;
        next();
    } catch (error) {
        return res.status(403).json({ message: 'Invalid or expired token' });
    }
}

export function getUserRole(user) {
    return user?.role || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
}

export function requireRole(...allowedRoles) {
    return (req, res, next) => {
        const role = getUserRole(req.user);

        if (!role || !allowedRoles.includes(role)) {
            return res.status(403).json({ message: 'No autorizado para este recurso' });
        }

        next();
    };
}

export function generateToken(payload) {
    //const expiration = process.env.JWT_EXPIRATION || '8h';
    const expiration = '8h';
    return jwt.sign(payload, JWT_SECRET, { expiresIn: expiration });
}
