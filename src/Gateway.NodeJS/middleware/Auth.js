/**
 * JWT Authentication Middleware
 */
import jwt from 'jsonwebtoken';

//const JWT_SECRET = process.env.JWT_SECRET || 'your_super_secret_jwt_key_change_this_in_production';
const JWT_SECRET = 'f9a2b8c7e6d5a4b3c2d1e0f9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b';
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

export function generateToken(payload) {
    //const expiration = process.env.JWT_EXPIRATION || '8h';
    const expiration = '30m';
    return jwt.sign(payload, JWT_SECRET, { expiresIn: expiration });
}
